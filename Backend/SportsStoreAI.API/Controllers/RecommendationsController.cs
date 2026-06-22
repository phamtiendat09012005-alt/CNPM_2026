using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Models;
using SportsStoreAI.API.Services;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RecommendationsController(ApplicationDbContext db) => _db = db;

    [HttpPost("views/{productId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> RecordView(
        int productId,
        RecordViewRequest request)
    {
        if (!await _db.Products.AnyAsync(x => x.Id == productId && x.IsActive))
            throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var userId = User.GetUserIdOrNull();
        if (!userId.HasValue && string.IsNullOrWhiteSpace(request.SessionId))
            request.SessionId = Guid.NewGuid().ToString("N");

        _db.ProductViewHistories.Add(new ProductViewHistory
        {
            UserId = userId,
            ProductId = productId,
            SessionId = request.SessionId?.Trim()
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { productId, request.SessionId }, "Đã ghi nhận lượt xem."));
    }

    [HttpGet("related/{productId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<RecommendationDto>>>> Related(
        int productId,
        [FromQuery] int take = 6)
    {
        var source = await _db.Products.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId && x.IsActive)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        take = Math.Clamp(take, 1, 12);
        var candidates = await _db.Products.AsNoTracking()
            .Where(x => x.IsActive && x.Id != source.Id &&
                        x.ProductVariants.Any(v => v.IsActive && v.StockQuantity > 0))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.CategoryId,
                x.BrandId,
                x.SportType,
                Price = x.ProductVariants.Where(v => v.IsActive)
                    .Select(v => (decimal?)v.Price).Min() ?? x.BasePrice,
                ImageUrl = x.ProductImages.OrderBy(i => i.SortOrder)
                    .Select(i => i.Url).FirstOrDefault(),
                Popularity = x.ViewHistories.Count
            })
            .Take(200)
            .ToListAsync();

        var result = candidates.Select(x =>
        {
            decimal score = 0;
            var reasons = new List<string>();
            if (x.CategoryId == source.CategoryId)
            {
                score += 0.45m;
                reasons.Add("cùng danh mục");
            }
            if (source.BrandId.HasValue && x.BrandId == source.BrandId)
            {
                score += 0.25m;
                reasons.Add("cùng thương hiệu");
            }
            if (!string.IsNullOrWhiteSpace(source.SportType) &&
                string.Equals(x.SportType, source.SportType, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.15m;
                reasons.Add("cùng môn thể thao");
            }
            var difference = Math.Abs(x.Price - source.BasePrice) / Math.Max(source.BasePrice, 1);
            score += 0.10m * (1 - Math.Min(difference, 1));
            score += Math.Min(x.Popularity / 1000m, 0.05m);

            return new RecommendationDto(
                x.Id,
                x.Name,
                x.Price,
                x.ImageUrl,
                Math.Round(score, 5),
                reasons.Count > 0
                    ? $"Phù hợp vì {string.Join(", ", reasons)}."
                    : "Sản phẩm phổ biến và đang còn hàng.");
        })
        .OrderByDescending(x => x.Score)
        .Take(take)
        .ToList();

        await LogRecommendations(result, User.GetUserIdOrNull(), productId, "Related");
        return Ok(ApiResponse<IReadOnlyCollection<RecommendationDto>>.Ok(result));
    }

    [HttpGet("personalized")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<RecommendationDto>>>> Personalized(
        [FromQuery] int take = 8)
    {
        var userId = User.GetRequiredUserId();
        take = Math.Clamp(take, 1, 12);
        var recent = await _db.ProductViewHistories.AsNoTracking()
            .Where(x => x.UserId == userId && x.ViewedAt >= DateTime.UtcNow.AddDays(-90))
            .Select(x => new { x.Product.CategoryId, x.Product.BrandId })
            .ToListAsync();

        var preferredCategories = recent.GroupBy(x => x.CategoryId)
            .OrderByDescending(x => x.Count()).Select(x => x.Key).Take(3).ToList();
        var preferredBrands = recent.Where(x => x.BrandId.HasValue)
            .GroupBy(x => x.BrandId!.Value)
            .OrderByDescending(x => x.Count()).Select(x => x.Key).Take(3).ToList();

        var query = _db.Products.AsNoTracking()
            .Where(x => x.IsActive && x.ProductVariants.Any(v => v.IsActive && v.StockQuantity > 0));

        var candidates = await query.Select(x => new
        {
            x.Id,
            x.Name,
            x.CategoryId,
            x.BrandId,
            Price = x.ProductVariants.Where(v => v.IsActive)
                .Select(v => (decimal?)v.Price).Min() ?? x.BasePrice,
            ImageUrl = x.ProductImages.OrderBy(i => i.SortOrder)
                .Select(i => i.Url).FirstOrDefault(),
            Popularity = x.ViewHistories.Count
        }).Take(200).ToListAsync();

        var result = candidates.Select(x =>
        {
            decimal score = 0.10m + Math.Min(x.Popularity / 1000m, 0.15m);
            var reason = "Sản phẩm phổ biến và đang còn hàng.";
            if (preferredCategories.Contains(x.CategoryId))
            {
                score += 0.50m;
                reason = "Phù hợp với danh mục bạn thường xem.";
            }
            if (x.BrandId.HasValue && preferredBrands.Contains(x.BrandId.Value))
            {
                score += 0.25m;
                reason = "Phù hợp với danh mục và thương hiệu bạn quan tâm.";
            }
            return new RecommendationDto(x.Id, x.Name, x.Price, x.ImageUrl,
                Math.Min(Math.Round(score, 5), 1m), reason);
        })
        .OrderByDescending(x => x.Score)
        .Take(take)
        .ToList();

        await LogRecommendations(result, userId, null, recent.Count == 0 ? "Popular" : "Personalized");
        return Ok(ApiResponse<IReadOnlyCollection<RecommendationDto>>.Ok(result));
    }

    [HttpPost("size")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SizeRecommendationDto>>> RecommendSize(
        SizeRecommendationRequest request)
    {
        var product = await _db.Products.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var charts = await _db.SizeCharts.AsNoTracking()
            .Where(x => x.IsActive && x.ProductType == product.ProductType &&
                        (!x.BrandId.HasValue || x.BrandId == product.BrandId))
            .ToListAsync();
        if (charts.Count == 0)
            throw new InvalidOperationException("Sản phẩm chưa có bảng kích thước.");

        SizeChart selected;
        int confidence;
        string reason;

        if (product.ProductType == "Shoes")
        {
            if (!request.FootLengthCm.HasValue)
                throw new ArgumentException("Vui lòng nhập chiều dài bàn chân.");
            selected = charts.Where(x => x.MinFootLengthCm.HasValue && x.MaxFootLengthCm.HasValue)
                .OrderBy(x => Distance(request.FootLengthCm.Value,
                    x.MinFootLengthCm!.Value, x.MaxFootLengthCm!.Value))
                .First();
            var distance = Distance(request.FootLengthCm.Value,
                selected.MinFootLengthCm!.Value, selected.MaxFootLengthCm!.Value);
            confidence = distance == 0 ? 90 : Math.Max(60, 85 - (int)(distance * 10));
            reason = $"Chiều dài bàn chân {request.FootLengthCm:0.##} cm phù hợp nhất với size {selected.SizeLabel}.";
        }
        else if (product.ProductType == "Clothing")
        {
            if (!request.HeightCm.HasValue && !request.WeightKg.HasValue &&
                !request.ChestCm.HasValue && !request.WaistCm.HasValue)
                throw new ArgumentException("Vui lòng nhập ít nhất một số đo cơ thể.");

            selected = charts.OrderBy(x =>
                OptionalDistance(request.HeightCm, x.MinHeightCm, x.MaxHeightCm) +
                OptionalDistance(request.WeightKg, x.MinWeightKg, x.MaxWeightKg) +
                OptionalDistance(request.ChestCm, x.MinChestCm, x.MaxChestCm) +
                OptionalDistance(request.WaistCm, x.MinWaistCm, x.MaxWaistCm))
                .First();
            confidence = 85;
            reason = $"Size {selected.SizeLabel} phù hợp nhất với các số đo đã cung cấp.";
        }
        else
        {
            throw new InvalidOperationException("Loại sản phẩm này không cần gợi ý kích thước.");
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredFit))
            reason += $" Kiểu mặc mong muốn: {request.PreferredFit.Trim()}.";

        var result = new SizeRecommendationDto(
            selected.SizeLabel,
            confidence,
            reason,
            "Kết quả gợi ý chỉ mang tính tham khảo. Hãy đối chiếu bảng kích thước trước khi mua.");

        _db.RecommendationLogs.Add(new RecommendationLog
        {
            UserId = User.GetUserIdOrNull(),
            RecommendationType = "Size",
            SourceProductId = product.Id,
            RecommendedSize = selected.SizeLabel,
            Score = confidence / 100m,
            Reason = reason
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<SizeRecommendationDto>.Ok(result));
    }

    private async Task LogRecommendations(
        IEnumerable<RecommendationDto> results,
        int? userId,
        int? sourceProductId,
        string type)
    {
        _db.RecommendationLogs.AddRange(results.Select(x => new RecommendationLog
        {
            UserId = userId,
            RecommendationType = type,
            SourceProductId = sourceProductId,
            RecommendedProductId = x.ProductId,
            Score = x.Score,
            Reason = x.Reason
        }));
        await _db.SaveChangesAsync();
    }

    private static decimal Distance(decimal value, decimal min, decimal max)
        => value < min ? min - value : value > max ? value - max : 0;

    private static decimal OptionalDistance(decimal? value, decimal? min, decimal? max)
        => value.HasValue && min.HasValue && max.HasValue
            ? Distance(value.Value, min.Value, max.Value)
            : 0;
}

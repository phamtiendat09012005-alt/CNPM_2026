using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsControllerTienDat_0460 : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductsControllerTienDat_0460(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductListDto>>>> GetAll(
        [FromQuery] ProductQuery query)
    {
        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 12;
        var products = _db.Products.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            products = products.Where(x =>
                EF.Functions.Like(x.Name, $"%{keyword}%") ||
                (x.Description != null && EF.Functions.Like(x.Description, $"%{keyword}%")));
        }

        // Đã thêm điều kiện > 0 để tránh lỗi lọc "Tất cả" từ Frontend
        if (query.CategoryId.HasValue && query.CategoryId.Value > 0)
            products = products.Where(x => x.CategoryId == query.CategoryId.Value);

        if (query.BrandId.HasValue && query.BrandId.Value > 0)
            products = products.Where(x => x.BrandId == query.BrandId.Value);

        if (!string.IsNullOrWhiteSpace(query.Size))
            products = products.Where(x => x.ProductVariants.Any(v => v.IsActive && v.Size == query.Size));

        if (!string.IsNullOrWhiteSpace(query.Color))
        {
            var color = query.Color.Trim();
            products = products.Where(x => x.ProductVariants.Any(v =>
                v.IsActive && v.Color != null && EF.Functions.Like(v.Color, $"%{color}%")));
        }

        if (query.MinPrice.HasValue && query.MinPrice.Value > 0)
            products = products.Where(x => x.ProductVariants.Any(v => v.IsActive && v.Price >= query.MinPrice.Value));

        if (query.MaxPrice.HasValue && query.MaxPrice.Value > 0)
            products = products.Where(x => x.ProductVariants.Any(v => v.IsActive && v.Price <= query.MaxPrice.Value));

        // Tối ưu hóa bằng Anonymous Type để tránh lỗi dịch EF Core
        var projected = products.Select(x => new
        {
            x.Id,
            x.Name,
            x.Slug,
            CategoryName = x.Category.Name,
            BrandName = x.Brand != null ? x.Brand.Name : null,
            MinPrice = x.ProductVariants.Where(v => v.IsActive).Select(v => (decimal?)v.Price).Min() ?? x.BasePrice,
            MaxPrice = x.ProductVariants.Where(v => v.IsActive).Select(v => (decimal?)v.Price).Max() ?? x.BasePrice,
            Stock = x.ProductVariants.Where(v => v.IsActive).Sum(v => (int?)v.StockQuantity) ?? 0,
            ImageUrl = x.ProductImages.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
            x.IsActive
        });

        projected = query.Sort?.ToLowerInvariant() switch
        {
            "price_asc" => projected.OrderBy(x => x.MinPrice),
            "price_desc" => projected.OrderByDescending(x => x.MaxPrice),
            "name" => projected.OrderBy(x => x.Name),
            _ => projected.OrderByDescending(x => x.Id)
        };

        var total = await projected.CountAsync();
        var rawItems = await projected.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        // Chuyển map sang DTO ở vòng lặp bộ nhớ, an toàn tuyệt đối
        var items = rawItems.Select(x => new ProductListDto(
            x.Id, x.Name, x.Slug, x.CategoryName, x.BrandName, x.MinPrice, x.MaxPrice, x.Stock, x.ImageUrl, x.IsActive
        )).ToList();

        var result = new PagedResult<ProductListDto>(
            items,
            page,
            pageSize,
            total,
            (int)Math.Ceiling(total / (double)pageSize));

        return Ok(ApiResponse<PagedResult<ProductListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.ProductImages)
            .Include(x => x.ProductVariants)
            .SingleOrDefaultAsync(x => x.Id == id && x.IsActive)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var data = new ProductDetailDto(
            product.Id,
            product.CategoryId,
            product.BrandId,
            product.Name,
            product.Slug,
            product.Description,
            product.ProductType,
            product.SportType,
            product.BasePrice,
            product.Category.Name,
            product.Brand?.Name,
            product.IsActive,
            product.ProductImages.OrderBy(x => x.SortOrder).Select(x => x.Url).ToList(),
            product.ProductVariants.Where(x => x.IsActive)
                .OrderBy(x => x.Size)
                .ThenBy(x => x.Color)
                .Select(x => new ProductVariantDto(
                    x.Id, x.Sku, x.Size, x.Color, x.Price, x.StockQuantity, x.IsActive))
                .ToList());

        return Ok(ApiResponse<ProductDetailDto>.Ok(data));
    }
}
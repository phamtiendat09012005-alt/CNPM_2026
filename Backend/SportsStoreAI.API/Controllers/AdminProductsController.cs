using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Models;
using SportsStoreAI.API.Services;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminProductsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll()
    {
        var data = await _db.Products.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ProductType,
                CategoryName = x.Category.Name,
                BrandName = x.Brand != null ? x.Brand.Name : null,
                Price = x.ProductVariants.Select(v => (decimal?)v.Price).Min() ?? x.BasePrice,
                Stock = x.ProductVariants.Sum(v => (int?)v.StockQuantity) ?? 0,
                ImageUrl = x.ProductImages.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                x.IsActive
            })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.ProductImages)
            .Include(x => x.ProductVariants)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var data = new ProductDetailDto(
            product.Id, product.CategoryId, product.BrandId, product.Name, product.Slug,
            product.Description, product.ProductType, product.SportType, product.BasePrice,
            product.Category.Name, product.Brand?.Name, product.IsActive,
            product.ProductImages.OrderBy(x => x.SortOrder).Select(x => x.Url).ToList(),
            product.ProductVariants.Select(x => new ProductVariantDto(
                x.Id, x.Sku, x.Size, x.Color, x.Price, x.StockQuantity, x.IsActive)).ToList());
        return Ok(ApiResponse<ProductDetailDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(ProductRequest request)
    {
        await ValidateRequest(request, null);
        var slug = await BuildUniqueSlug(request.Name, null);
        var entity = new Product
        {
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            ProductType = request.ProductType,
            SportType = request.SportType?.Trim(),
            BasePrice = request.BasePrice,
            IsActive = request.IsActive
        };

        AddImages(entity, request.ImageUrls);
        foreach (var item in request.Variants)
        {
            entity.ProductVariants.Add(new ProductVariant
            {
                Sku = item.Sku.Trim().ToUpperInvariant(),
                Size = item.Size?.Trim(),
                Color = item.Color?.Trim(),
                Price = item.Price,
                StockQuantity = item.StockQuantity,
                IsActive = item.IsActive
            });
        }

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Tạo sản phẩm thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, ProductRequest request)
    {
        var entity = await _db.Products
            .Include(x => x.ProductImages)
            .Include(x => x.ProductVariants)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        await ValidateRequest(request, id);
        entity.CategoryId = request.CategoryId;
        entity.BrandId = request.BrandId;
        entity.Name = request.Name.Trim();
        entity.Slug = await BuildUniqueSlug(request.Name, id);
        entity.Description = request.Description?.Trim();
        entity.ProductType = request.ProductType;
        entity.SportType = request.SportType?.Trim();
        entity.BasePrice = request.BasePrice;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.ProductImages.RemoveRange(entity.ProductImages);
        entity.ProductImages.Clear();
        AddImages(entity, request.ImageUrls);

        foreach (var variantRequest in request.Variants)
        {
            ProductVariant variant;
            if (variantRequest.Id.HasValue)
            {
                variant = entity.ProductVariants.SingleOrDefault(x => x.Id == variantRequest.Id.Value)
                    ?? throw new InvalidOperationException("Biến thể không thuộc sản phẩm này.");
            }
            else
            {
                variant = new ProductVariant { ProductId = entity.Id };
                entity.ProductVariants.Add(variant);
            }

            variant.Sku = variantRequest.Sku.Trim().ToUpperInvariant();
            variant.Size = variantRequest.Size?.Trim();
            variant.Color = variantRequest.Color?.Trim();
            variant.Price = variantRequest.Price;
            variant.StockQuantity = variantRequest.StockQuantity;
            variant.IsActive = variantRequest.IsActive;
            variant.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Cập nhật sản phẩm thành công."));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<object>>> SetStatus(int id, [FromBody] bool isActive)
    {
        var entity = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.IsActive }, "Cập nhật trạng thái thành công."));
    }

    private async Task ValidateRequest(ProductRequest request, int? productId)
    {
        if (!await _db.Categories.AnyAsync(x => x.Id == request.CategoryId))
            throw new InvalidOperationException("Danh mục không tồn tại.");
        if (request.BrandId.HasValue && !await _db.Brands.AnyAsync(x => x.Id == request.BrandId.Value))
            throw new InvalidOperationException("Thương hiệu không tồn tại.");
        if (request.Variants.Count == 0)
            throw new InvalidOperationException("Sản phẩm cần ít nhất một biến thể.");

        var skus = request.Variants.Select(x => x.Sku.Trim().ToUpperInvariant()).ToList();
        if (skus.Distinct().Count() != skus.Count)
            throw new InvalidOperationException("SKU trong sản phẩm bị trùng.");

        var ids = request.Variants.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var duplicated = productId.HasValue
            ? await _db.ProductVariants.AnyAsync(x =>
                skus.Contains(x.Sku) &&
                (x.ProductId != productId.Value || !ids.Contains(x.Id)))
            : await _db.ProductVariants.AnyAsync(x => skus.Contains(x.Sku));

        if (duplicated)
            throw new InvalidOperationException("Một hoặc nhiều SKU đã tồn tại.");
    }

    private async Task<string> BuildUniqueSlug(string name, int? currentId)
    {
        var baseSlug = SlugService.Create(name);
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? "san-pham" : baseSlug;
        var current = slug;
        var suffix = 1;
        while (await _db.Products.AnyAsync(x => x.Slug == current && (!currentId.HasValue || x.Id != currentId)))
            current = $"{slug}-{suffix++}";
        return current;
    }

    private static void AddImages(Product product, IEnumerable<string> urls)
    {
        var index = 0;
        foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            product.ProductImages.Add(new ProductImage
            {
                Url = url.Trim(),
                IsPrimary = index == 0,
                SortOrder = index++
            });
        }
    }
}

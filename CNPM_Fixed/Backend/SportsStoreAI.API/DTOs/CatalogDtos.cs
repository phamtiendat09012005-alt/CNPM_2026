using System.ComponentModel.DataAnnotations;

namespace SportsStoreAI.API.DTOs;

public sealed class ProductQuery
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public sealed record ProductListDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string? BrandName,
    decimal MinPrice,
    decimal MaxPrice,
    int TotalStock,
    string? ImageUrl,
    bool IsActive);

public sealed record ProductVariantDto(
    int Id,
    string Sku,
    string? Size,
    string? Color,
    decimal Price,
    int StockQuantity,
    bool IsActive);

public sealed record ProductDetailDto(
    int Id,
    int CategoryId,
    int? BrandId,
    string Name,
    string Slug,
    string? Description,
    string ProductType,
    string? SportType,
    decimal BasePrice,
    string CategoryName,
    string? BrandName,
    bool IsActive,
    IReadOnlyCollection<string> Images,
    IReadOnlyCollection<ProductVariantDto> Variants);

public sealed class CategoryRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public sealed class ProductVariantRequest
{
    public int? Id { get; set; }

    [Required, MaxLength(60)]
    public string Sku { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Size { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class ProductRequest
{
    [Required]
    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    [Required, MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(30)]
    public string ProductType { get; set; } = "Equipment";

    [MaxLength(80)]
    public string? SportType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantRequest> Variants { get; set; } = new();
}

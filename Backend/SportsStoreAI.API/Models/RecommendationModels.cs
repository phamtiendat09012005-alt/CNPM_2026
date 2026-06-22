namespace SportsStoreAI.API.Models;

public sealed class ProductViewHistory
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public int ProductId { get; set; }
    public string? SessionId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser? User { get; set; }
    public Product Product { get; set; } = null!;
}

public sealed class SizeChart
{
    public int Id { get; set; }
    public int? BrandId { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string SizeLabel { get; set; } = string.Empty;
    public decimal? MinFootLengthCm { get; set; }
    public decimal? MaxFootLengthCm { get; set; }
    public decimal? MinHeightCm { get; set; }
    public decimal? MaxHeightCm { get; set; }
    public decimal? MinWeightKg { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? MinChestCm { get; set; }
    public decimal? MaxChestCm { get; set; }
    public decimal? MinWaistCm { get; set; }
    public decimal? MaxWaistCm { get; set; }
    public string? FitNote { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Brand? Brand { get; set; }
}

public sealed class RecommendationLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public string RecommendationType { get; set; } = string.Empty;
    public int? SourceProductId { get; set; }
    public int? RecommendedProductId { get; set; }
    public string? RecommendedSize { get; set; }
    public decimal Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsClicked { get; set; }
    public bool IsPurchased { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser? User { get; set; }
    public Product? SourceProduct { get; set; }
    public Product? RecommendedProduct { get; set; }
}

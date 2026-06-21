using System.ComponentModel.DataAnnotations;

namespace SportsStoreAI.API.DTOs;

public sealed class RecordViewRequest
{
    [MaxLength(100)]
    public string? SessionId { get; set; }
}

public sealed record RecommendationDto(
    int ProductId,
    string ProductName,
    decimal Price,
    string? ImageUrl,
    decimal Score,
    string Reason);

public sealed class SizeRecommendationRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public decimal? FootLengthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? WaistCm { get; set; }

    [MaxLength(20)]
    public string? PreferredFit { get; set; }
}

public sealed record SizeRecommendationDto(
    string RecommendedSize,
    int Confidence,
    string Reason,
    string Disclaimer);

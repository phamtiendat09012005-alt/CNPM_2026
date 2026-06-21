using System.ComponentModel.DataAnnotations;

namespace SportsStoreAI.API.DTOs;

public sealed class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;
}

public sealed class UpdateCartItemRequest
{
    [Range(1, 99)]
    public int Quantity { get; set; }

    public bool IsSelected { get; set; } = true;
}

public sealed record CartItemDto(
    int Id,
    int ProductId,
    int ProductVariantId,
    string ProductName,
    string Sku,
    string? Size,
    string? Color,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    int StockQuantity,
    bool IsSelected,
    decimal LineTotal);

public sealed record CartDto(
    int Id,
    IReadOnlyCollection<CartItemDto> Items,
    decimal SelectedTotal,
    int SelectedQuantity);

public sealed class CheckoutRequest
{
    [Required, MaxLength(120)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ReceiverPhone { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = "COD";

    [MaxLength(500)]
    public string? Note { get; set; }
}

public sealed record OrderItemDto(
    long Id,
    int ProductVariantId,
    string ProductName,
    string Sku,
    string? Size,
    string? Color,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record PaymentDto(
    long Id,
    string Method,
    string Status,
    decimal Amount,
    string? ProviderTransactionId,
    DateTime? PaidAt);

public sealed record OrderDto(
    long Id,
    string OrderCode,
    int UserId,
    string CustomerName,
    string ReceiverName,
    string ReceiverPhone,
    string ShippingAddress,
    decimal Subtotal,
    decimal ShippingFee,
    decimal TotalAmount,
    string Status,
    string? Note,
    DateTime CreatedAt,
    IReadOnlyCollection<OrderItemDto> Items,
    PaymentDto? Payment,
    IReadOnlyCollection<string> StatusHistory);

public sealed class ChangeOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }
}

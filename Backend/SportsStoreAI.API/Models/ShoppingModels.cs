namespace SportsStoreAI.API.Models;

public sealed class UserAddress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
}

public sealed class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public sealed class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public bool IsSelected { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Cart Cart { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}

public sealed class Order
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
}

public sealed class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; private set; }
    public Order Order { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}

public sealed class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string Method { get; set; } = "COD";
    public string Status { get; set; } = "Unpaid";
    public decimal Amount { get; set; }
    public string? Provider { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Order Order { get; set; } = null!;
}

public sealed class OrderStatusHistory
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Order Order { get; set; } = null!;
    public ApplicationUser? ChangedByUser { get; set; }
}

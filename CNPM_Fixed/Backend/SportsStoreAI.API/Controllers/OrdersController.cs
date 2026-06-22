using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Models;
using SportsStoreAI.API.Services;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(CheckoutRequest request)
    {
        var method = request.PaymentMethod.Trim();
        if (method is not ("COD" or "OnlineGateway"))
            return BadRequest(ApiResponse<object>.Fail("Phương thức thanh toán không hợp lệ."));

        var userId = User.GetRequiredUserId();
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var cart = await _db.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.UserId == userId)
            ?? throw new InvalidOperationException("Giỏ hàng chưa được tạo.");

        var selectedItems = cart.CartItems.Where(x => x.IsSelected).ToList();
        if (selectedItems.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("Chưa chọn sản phẩm để thanh toán."));

        foreach (var item in selectedItems)
        {
            if (!item.ProductVariant.IsActive || !item.ProductVariant.Product.IsActive)
                return BadRequest(ApiResponse<object>.Fail($"Sản phẩm {item.ProductVariant.Sku} đã ngừng bán."));
            if (item.ProductVariant.StockQuantity < item.Quantity)
                return BadRequest(ApiResponse<object>.Fail($"Sản phẩm {item.ProductVariant.Sku} không đủ tồn kho."));
        }

        var subtotal = selectedItems.Sum(x => x.ProductVariant.Price * x.Quantity);
        var shippingFee = subtotal >= 1_000_000 ? 0 : 30_000;
        var initialOrderStatus = method == "OnlineGateway" ? "AwaitingPayment" : "Pending";
        var initialPaymentStatus = method == "OnlineGateway" ? "Pending" : "Unpaid";

        var order = new Order
        {
            UserId = userId,
            OrderCode = $"SSA{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            ReceiverName = request.ReceiverName.Trim(),
            ReceiverPhone = request.ReceiverPhone.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            TotalAmount = subtotal + shippingFee,
            Status = initialOrderStatus,
            Note = request.Note?.Trim()
        };

        foreach (var item in selectedItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductVariant.Product.Name,
                Sku = item.ProductVariant.Sku,
                Size = item.ProductVariant.Size,
                Color = item.ProductVariant.Color,
                UnitPrice = item.ProductVariant.Price,
                Quantity = item.Quantity
            });
            item.ProductVariant.StockQuantity -= item.Quantity;
        }

        order.Payments.Add(new Payment
        {
            Method = method,
            Status = initialPaymentStatus,
            Amount = order.TotalAmount,
            Provider = method == "OnlineGateway" ? "DemoGateway" : null
        });
        order.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Status = initialOrderStatus,
            ChangedByUserId = userId,
            Note = "Khách hàng tạo đơn."
        });

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(selectedItems);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            order.Id,
            order.OrderCode,
            order.TotalAmount,
            order.Status,
            PaymentMethod = method,
            RequiresDemoPayment = method == "OnlineGateway"
        }, "Tạo đơn hàng thành công."));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<object>>> GetMyOrders()
    {
        var userId = User.GetRequiredUserId();
        var data = await _db.Orders.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.OrderCode,
                x.TotalAmount,
                x.Status,
                PaymentStatus = x.Payments.OrderByDescending(p => p.Id).Select(p => p.Status).FirstOrDefault(),
                x.CreatedAt,
                ItemCount = x.OrderItems.Sum(i => i.Quantity)
            })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(long id)
    {
        var userId = User.GetRequiredUserId();
        var isAdmin = User.IsInRole("Admin");
        var order = await QueryOrder().SingleOrDefaultAsync(x => x.Id == id && (isAdmin || x.UserId == userId))
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        return Ok(ApiResponse<OrderDto>.Ok(MapOrder(order)));
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(long id)
    {
        var userId = User.GetRequiredUserId();
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await _db.Orders
            .Include(x => x.OrderItems).ThenInclude(x => x.ProductVariant)
            .Include(x => x.Payments)
            .Include(x => x.OrderStatusHistories)
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.Status is not ("AwaitingPayment" or "Pending" or "Confirmed"))
            return BadRequest(ApiResponse<object>.Fail("Đơn hàng không còn được phép hủy."));

        foreach (var item in order.OrderItems)
            item.ProductVariant.StockQuantity += item.Quantity;

        order.Status = "Cancelled";
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        var payment = order.Payments.OrderByDescending(x => x.Id).FirstOrDefault();
        if (payment is not null)
        {
            payment.Status = payment.Status == "Paid" ? "Refunded" : "Failed";
            payment.UpdatedAt = DateTime.UtcNow;
        }
        order.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Status = "Cancelled",
            ChangedByUserId = userId,
            Note = "Khách hàng hủy đơn."
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(ApiResponse<object>.Ok(new { order.Id, order.Status }, "Đã hủy đơn hàng."));
    }

    [HttpPost("{id:long}/simulate-payment")]
    public async Task<ActionResult<ApiResponse<object>>> SimulatePayment(long id)
    {
        var userId = User.GetRequiredUserId();
        var order = await _db.Orders.Include(x => x.Payments)
            .Include(x => x.OrderStatusHistories)
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var payment = order.Payments.OrderByDescending(x => x.Id).FirstOrDefault()
            ?? throw new InvalidOperationException("Đơn hàng chưa có thanh toán.");
        if (payment.Method != "OnlineGateway")
            return BadRequest(ApiResponse<object>.Fail("Đơn hàng không sử dụng thanh toán online."));
        if (payment.Status == "Paid")
            return Ok(ApiResponse<object>.Ok(new { order.Id, order.Status }, "Đơn hàng đã thanh toán."));

        payment.Status = "Paid";
        payment.ProviderTransactionId = $"DEMO-{Guid.NewGuid():N}";
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        order.Status = "Pending";
        order.UpdatedAt = DateTime.UtcNow;
        order.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Status = "Pending",
            ChangedByUserId = userId,
            Note = "Thanh toán online mô phỏng thành công."
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { order.Id, order.Status, PaymentStatus = payment.Status }, "Thanh toán mô phỏng thành công."));
    }

    private IQueryable<Order> QueryOrder() => _db.Orders.AsNoTracking()
        .Include(x => x.User)
        .Include(x => x.OrderItems)
        .Include(x => x.Payments)
        .Include(x => x.OrderStatusHistories);

    internal static OrderDto MapOrder(Order order)
    {
        var payment = order.Payments.OrderByDescending(x => x.Id).FirstOrDefault();
        return new OrderDto(
            order.Id,
            order.OrderCode,
            order.UserId,
            order.User.FullName,
            order.ReceiverName,
            order.ReceiverPhone,
            order.ShippingAddress,
            order.Subtotal,
            order.ShippingFee,
            order.TotalAmount,
            order.Status,
            order.Note,
            order.CreatedAt,
            order.OrderItems.Select(x => new OrderItemDto(
                x.Id, x.ProductVariantId, x.ProductName, x.Sku, x.Size, x.Color,
                x.UnitPrice, x.Quantity, x.UnitPrice * x.Quantity)).ToList(),
            payment is null ? null : new PaymentDto(
                payment.Id, payment.Method, payment.Status, payment.Amount,
                payment.ProviderTransactionId, payment.PaidAt),
            order.OrderStatusHistories.OrderBy(x => x.ChangedAt)
                .Select(x => $"{x.ChangedAt:dd/MM/yyyy HH:mm} - {x.Status}").ToList());
    }
}

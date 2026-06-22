using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Services;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminOrdersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? status)
    {
        var query = _db.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.OrderCode,
                CustomerName = x.User.FullName,
                x.TotalAmount,
                x.Status,
                PaymentMethod = x.Payments.OrderByDescending(p => p.Id).Select(p => p.Method).FirstOrDefault(),
                PaymentStatus = x.Payments.OrderByDescending(p => p.Id).Select(p => p.Status).FirstOrDefault(),
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(long id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .Include(x => x.Payments)
            .Include(x => x.OrderStatusHistories)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        return Ok(ApiResponse<OrderDto>.Ok(OrdersController.MapOrder(order)));
    }

    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<object>>> ChangeStatus(
        long id,
        ChangeOrderStatusRequest request)
    {
        var next = request.Status.Trim();
        var order = await _db.Orders
            .Include(x => x.Payments)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.ProductVariant)
            .Include(x => x.OrderStatusHistories)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        OrderWorkflow.EnsureCanMove(order.Status, next);
        if (next == "Cancelled")
        {
            foreach (var item in order.OrderItems)
            {
                item.ProductVariant.StockQuantity += item.Quantity;
            }
            order.CancelledAt = DateTime.UtcNow;

            var cancelledPayment = order.Payments.OrderByDescending(x => x.Id).FirstOrDefault();
            if (cancelledPayment is not null)
            {
                cancelledPayment.Status = cancelledPayment.Status == "Paid" ? "Refunded" : "Failed";
                cancelledPayment.UpdatedAt = DateTime.UtcNow;
            }
        }

        order.Status = next;
        order.UpdatedAt = DateTime.UtcNow;
        order.OrderStatusHistories.Add(new SportsStoreAI.API.Models.OrderStatusHistory
        {
            Status = next,
            ChangedByUserId = User.GetRequiredUserId(),
            Note = request.Note?.Trim()
        });

        if (next == "Completed")
        {
            var payment = order.Payments.OrderByDescending(x => x.Id).FirstOrDefault();
            if (payment is not null && payment.Method == "COD" && payment.Status == "Unpaid")
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { order.Id, order.Status }, "Cập nhật trạng thái thành công."));
    }
}

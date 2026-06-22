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
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CartController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
    {
        var cart = await GetOrCreateCart(User.GetRequiredUserId());
        return Ok(ApiResponse<CartDto>.Ok(MapCart(cart)));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem(AddCartItemRequest request)
    {
        var userId = User.GetRequiredUserId();
        var variant = await _db.ProductVariants
            .Include(x => x.Product)
            .SingleOrDefaultAsync(x =>
                x.Id == request.ProductVariantId && x.IsActive && x.Product.IsActive)
            ?? throw new KeyNotFoundException("Không tìm thấy biến thể sản phẩm.");

        if (variant.StockQuantity < request.Quantity)
            return BadRequest(ApiResponse<CartDto>.Fail("Số lượng tồn kho không đủ."));

        var cart = await GetOrCreateCart(userId);
        var existing = cart.CartItems.SingleOrDefault(x => x.ProductVariantId == request.ProductVariantId);
        if (existing is null)
        {
            cart.CartItems.Add(new CartItem
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                IsSelected = true
            });
        }
        else
        {
            var newQuantity = existing.Quantity + request.Quantity;
            if (newQuantity > variant.StockQuantity)
                return BadRequest(ApiResponse<CartDto>.Fail("Số lượng trong giỏ vượt quá tồn kho."));
            existing.Quantity = newQuantity;
            existing.IsSelected = true;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        cart = await LoadCart(cart.Id);
        return Ok(ApiResponse<CartDto>.Ok(MapCart(cart), "Đã thêm sản phẩm vào giỏ."));
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateItem(
        int itemId,
        UpdateCartItemRequest request)
    {
        var userId = User.GetRequiredUserId();
        var item = await _db.CartItems
            .Include(x => x.Cart)
            .Include(x => x.ProductVariant)
            .SingleOrDefaultAsync(x => x.Id == itemId && x.Cart.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm trong giỏ.");

        if (request.Quantity > item.ProductVariant.StockQuantity)
            return BadRequest(ApiResponse<CartDto>.Fail("Số lượng vượt quá tồn kho."));

        item.Quantity = request.Quantity;
        item.IsSelected = request.IsSelected;
        item.UpdatedAt = DateTime.UtcNow;
        item.Cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var cart = await LoadCart(item.CartId);
        return Ok(ApiResponse<CartDto>.Ok(MapCart(cart), "Cập nhật giỏ hàng thành công."));
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> DeleteItem(int itemId)
    {
        var userId = User.GetRequiredUserId();
        var item = await _db.CartItems
            .Include(x => x.Cart)
            .SingleOrDefaultAsync(x => x.Id == itemId && x.Cart.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm trong giỏ.");

        var cartId = item.CartId;
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        var cart = await LoadCart(cartId);
        return Ok(ApiResponse<CartDto>.Ok(MapCart(cart), "Đã xóa sản phẩm khỏi giỏ."));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> Clear()
    {
        var userId = User.GetRequiredUserId();
        var cart = await _db.Carts.Include(x => x.CartItems)
            .SingleOrDefaultAsync(x => x.UserId == userId);
        if (cart is not null)
        {
            _db.CartItems.RemoveRange(cart.CartItems);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Đã xóa toàn bộ giỏ hàng."));
    }

    private async Task<Cart> GetOrCreateCart(int userId)
    {
        var cart = await _db.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.ProductImages)
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (cart is not null)
            return cart;

        cart = new Cart { UserId = userId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    private Task<Cart> LoadCart(int cartId)
        => _db.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.ProductImages)
            .SingleAsync(x => x.Id == cartId);

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.CartItems
            .OrderByDescending(x => x.Id)
            .Select(x =>
            {
                var product = x.ProductVariant.Product;
                return new CartItemDto(
                    x.Id,
                    product.Id,
                    x.ProductVariantId,
                    product.Name,
                    x.ProductVariant.Sku,
                    x.ProductVariant.Size,
                    x.ProductVariant.Color,
                    product.ProductImages.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                    x.ProductVariant.Price,
                    x.Quantity,
                    x.ProductVariant.StockQuantity,
                    x.IsSelected,
                    x.ProductVariant.Price * x.Quantity);
            })
            .ToList();

        return new CartDto(
            cart.Id,
            items,
            items.Where(x => x.IsSelected).Sum(x => x.LineTotal),
            items.Where(x => x.IsSelected).Sum(x => x.Quantity));
    }
}

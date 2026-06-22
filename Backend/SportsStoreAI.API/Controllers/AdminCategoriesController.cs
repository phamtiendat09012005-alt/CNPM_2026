using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Models;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminCategoriesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll()
    {
        var data = await _db.Categories.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Description, x.IsActive })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(CategoryRequest request)
    {
        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(x => x.Name == name))
            return BadRequest(ApiResponse<object>.Fail("Tên danh mục đã tồn tại."));

        var entity = new Category { Name = name, Description = request.Description?.Trim() };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Name, entity.Description, entity.IsActive }, "Tạo danh mục thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, CategoryRequest request)
    {
        var entity = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");
        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(x => x.Id != id && x.Name == name))
            return BadRequest(ApiResponse<object>.Fail("Tên danh mục đã tồn tại."));

        entity.Name = name;
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.Name, entity.Description, entity.IsActive }, "Cập nhật danh mục thành công."));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<object>>> SetStatus(int id, [FromBody] bool isActive)
    {
        var entity = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");
        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.IsActive }, "Cập nhật trạng thái thành công."));
    }
}

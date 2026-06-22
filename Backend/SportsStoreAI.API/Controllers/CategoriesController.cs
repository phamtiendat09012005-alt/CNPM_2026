using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Data;
using SportsStoreAI.API.DTOs;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesControllerTienDat_0460 : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesControllerTienDat_0460(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll()
    {
        var data = await _db.Categories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(data));
    }
}
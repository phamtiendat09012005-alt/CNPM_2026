using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Models;

namespace SportsStoreAI.API.Data;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        foreach (var role in new[] { "Customer", "Admin" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var email = _configuration["SeedAdmin:Email"] ?? "admin@sportsstoreai.local";
        var password = _configuration["SeedAdmin:Password"] ?? "Admin@123456";
        var admin = await _userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Quản trị viên",
                Status = "Active"
            };

            var result = await _userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, "Admin"))
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        var seedUsers = new[]
        {
            new { FullName = "Phạm Tiến Đạt", Email = "tiendat.2380600460@sportsstoreai.local", Password = "User@2380600460" },
            new { FullName = "Lê Trần Quốc Thịnh", Email = "quocthinh.2380602127@sportsstoreai.local", Password = "User@2380602127" },
            new { FullName = "Hồ Ngọc Huy", Email = "ngochuy.2380600809@sportsstoreai.local", Password = "User@2380600809" },
            new { FullName = "Nguyễn Minh Thuận", Email = "minhthuan.2380602165@sportsstoreai.local", Password = "User@2380602165" }
        };

        foreach (var u in seedUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(u.Email);
            if (existingUser is null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FullName = u.FullName,
                    EmailConfirmed = true,
                    Status = "Active"
                };

                var result = await _userManager.CreateAsync(newUser, u.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Customer");
                }
            }
        }

        if (!await _db.Categories.AnyAsync())
        {
            _db.Categories.AddRange(
                new Category { Name = "Giày thể thao", Description = "Giày chạy bộ và luyện tập." },
                new Category { Name = "Quần áo thể thao", Description = "Áo và quần thể thao." },
                new Category { Name = "Dụng cụ thể thao", Description = "Bóng, vợt và dụng cụ." },
                new Category { Name = "Phụ kiện", Description = "Balo, tất và phụ kiện." });
        }

        if (!await _db.Brands.AnyAsync())
        {
            _db.Brands.AddRange(
                new Brand { Name = "Nike" },
                new Brand { Name = "Adidas" },
                new Brand { Name = "Puma" },
                new Brand { Name = "Yonex" });
        }

        await _db.SaveChangesAsync();
    }
}
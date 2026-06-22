using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsStoreAI.API.DTOs;
using SportsStoreAI.API.Models;
using SportsStoreAI.API.Services;

namespace SportsStoreAI.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthControllerTienDat_0460 : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthControllerTienDat_0460(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("Email đã được sử dụng."));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            EmailConfirmed = true,
            Status = "Active"
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                string.Join("; ", result.Errors.Select(x => x.Description))));
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.Create(user, roles);

        return Ok(ApiResponse<AuthResponse>.Ok(
            new AuthResponse(user.Id, user.FullName, email, roles.ToList(), token.Token, token.ExpiresAtUtc),
            "Đăng ký thành công."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || user.Status != "Active")
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.Create(user, roles);

        return Ok(ApiResponse<AuthResponse>.Ok(
            new AuthResponse(user.Id, user.FullName, user.Email!, roles.ToList(), token.Token, token.ExpiresAtUtc),
            "Đăng nhập thành công."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Me()
    {
        var user = await _userManager.FindByIdAsync(User.GetRequiredUserId().ToString())
            ?? throw new KeyNotFoundException("Không tìm thấy tài khoản.");
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Status,
            Roles = roles
        }));
    }
}
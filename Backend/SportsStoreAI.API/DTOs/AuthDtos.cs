using System.ComponentModel.DataAnnotations;

namespace SportsStoreAI.API.DTOs;

public sealed class RegisterRequest
{
    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthResponse(
    int UserId,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    string Token,
    DateTime ExpiresAtUtc);

using System.Security.Claims;

namespace SportsStoreAI.API.Services;

public static class CurrentUserExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id))
        {
            throw new UnauthorizedAccessException("Token người dùng không hợp lệ.");
        }

        return id;
    }

    public static int? GetUserIdOrNull(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}

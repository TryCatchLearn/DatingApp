using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        try
        {
            return Convert.ToInt32(user.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
        catch
        {
            return null;
        }
    }
}

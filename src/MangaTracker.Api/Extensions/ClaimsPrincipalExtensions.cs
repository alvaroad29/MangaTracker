using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MangaTracker.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? throw new InvalidOperationException("El token no contiene un claim de identificador de usuario ('sub' o NameIdentifier).");

        return Guid.Parse(value);
    }
}
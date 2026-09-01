using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MangaTracker.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? throw new InvalidOperationException("El token no contiene un claim 'sub'.");

        return Guid.Parse(value);
    }
}
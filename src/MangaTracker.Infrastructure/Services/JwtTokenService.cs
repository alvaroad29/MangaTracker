using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MangaTracker.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MangaTracker.Infrastructure.Services;

public class JwtTokenService(IConfiguration config) : ITokenService
{
    public (string Token, DateTime ExpiresAt) GenerateAccessToken(Guid userId, string email)
    {
        var secret = config["Jwt:Secret"]!;
        var issuer = config["Jwt:Issuer"]!;
        var audience = config["Jwt:Audience"]!;
        var minutes = int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}


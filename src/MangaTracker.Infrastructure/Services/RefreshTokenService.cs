using System.Security.Cryptography;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MangaTracker.Infrastructure.Services;

public class RefreshTokenService(ApplicationDbContext db, IConfiguration config) : IRefreshTokenService
{
    public async Task<string> GenerateAsync(Guid userId)
    {
        var plainToken = GenerateSecureToken();
        var days = int.Parse(config["Jwt:RefreshTokenExpirationDays"]!);

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(plainToken),
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return plainToken;
    }

    public async Task<RefreshTokenResult> ValidateAndRotateAsync(string plainToken)
    {
        var hash = Hash(plainToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (existing is null)
            return new RefreshTokenResult(false, Guid.Empty, null, "Token inválido.");

        if (existing.RevokedAt is not null)
        {
            // El token ya fue usado antes (rotado) y alguien intenta reusarlo → señal de robo.
            // Revocamos TODOS los refresh tokens activos de este usuario por seguridad.
            var activeTokens = await db.RefreshTokens
                .Where(x => x.UserId == existing.UserId && x.RevokedAt == null)
                .ToListAsync();

            foreach (var t in activeTokens) t.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return new RefreshTokenResult(false, Guid.Empty, null, "Token reutilizado. Sesiones revocadas por seguridad.");
        }

        if (existing.ExpiresAt < DateTime.UtcNow)
            return new RefreshTokenResult(false, Guid.Empty, null, "Token expirado.");

        var newPlainToken = GenerateSecureToken();
        var newHash = Hash(newPlainToken);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenExpirationDays"]!)),
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return new RefreshTokenResult(true, existing.UserId, newPlainToken, null);
    }

    public async Task RevokeAsync(string plainToken)
    {
        var hash = Hash(plainToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
        if (existing is null || existing.RevokedAt is not null) return;

        existing.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
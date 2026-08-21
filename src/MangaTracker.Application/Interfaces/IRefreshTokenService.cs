namespace MangaTracker.Application.Interfaces;

public record RefreshTokenResult(bool Success, Guid UserId, string? NewPlainToken, string? FailureReason);

public interface IRefreshTokenService
{
    Task<string> GenerateAsync(Guid userId);
    Task<RefreshTokenResult> ValidateAndRotateAsync(string plainToken);
    Task RevokeAsync(string plainToken);
}
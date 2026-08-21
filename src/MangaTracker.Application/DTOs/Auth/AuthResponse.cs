namespace MangaTracker.Application.DTOs.Auth;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserDto User);
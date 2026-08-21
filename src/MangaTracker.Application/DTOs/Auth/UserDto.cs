namespace MangaTracker.Application.DTOs.Auth;

public record UserDto(Guid Id, string Email, string DisplayName, string? AvatarUrl);
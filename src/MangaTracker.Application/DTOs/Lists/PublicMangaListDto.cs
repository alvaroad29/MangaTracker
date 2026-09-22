namespace MangaTracker.Application.DTOs.Lists;

public record PublicMangaListDto(
    Guid Id,
    string Name,
    string? Description,
    int MangaCount,
    int FollowerCount,
    Guid OwnerId,
    string OwnerDisplayName,
    string? OwnerAvatarUrl,
    bool IsFollowedByMe,
    DateTime CreatedAt);
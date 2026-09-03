namespace MangaTracker.Application.DTOs.Lists;

public record PublicMangaListDto(
    Guid Id,
    string Name,
    string? Description,
    int ItemCount,
    int FollowerCount,
    Guid OwnerId,
    string OwnerDisplayName,
    string? OwnerAvatarUrl,
    bool IsFollowedByMe,
    DateTime CreatedAt);
namespace MangaTracker.Application.DTOs.Lists;

public record MangaListDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsPublic,
    Guid OwnerId,
    string OwnerDisplayName,
    string? OwnerAvatarUrl,
    int FollowerCount,
    bool IsFollowedByMe,
    DateTime CreatedAt,
    IReadOnlyList<MangaListItemDto> Items);
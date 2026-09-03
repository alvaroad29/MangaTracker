using MangaTracker.Application.DTOs.Lists;

namespace MangaTracker.Application.Interfaces;

public interface IMangaListSocialService
{
    Task<IReadOnlyList<PublicMangaListDto>> SearchPublicListsAsync(Guid? currentUserId, string? search, int page);
    Task<IReadOnlyList<PublicMangaListDto>> GetUserPublicListsAsync(Guid targetUserId, Guid? currentUserId);
    Task<IReadOnlyList<PublicMangaListDto>> GetFollowedListsAsync(Guid userId);
    Task FollowAsync(Guid userId, Guid listId);
    Task UnfollowAsync(Guid userId, Guid listId);
}
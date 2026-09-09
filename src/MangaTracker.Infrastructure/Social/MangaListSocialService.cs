using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Infrastructure.Identity;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Social;

public class MangaListSocialService(ApplicationDbContext db) : IMangaListSocialService
{
    private const int PageSize = 20;
       

    public async Task<IReadOnlyList<PublicMangaListDto>> SearchPublicListsAsync(Guid? currentUserId, string? search, int page)
    {
         var query =
            from list in db.MangaLists
            join owner in db.Users on list.UserId equals owner.Id
            where list.IsPublic
            select new { list, owner };

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.list.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.list.Name.Contains(search));

        var paged = await query
            .OrderByDescending(x => x.list.Followers.Count)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(x => new PublicListProjection(
                x.list,
                x.owner.Id,
                x.owner.DisplayName,
                x.owner.AvatarUrl,
                x.list.MangaListItems.Count,
                x.list.Followers.Count,
                currentUserId != null && x.list.Followers.Any(f => f.FollowerUserId == currentUserId)
            ))
            .ToListAsync();

        return paged.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PublicMangaListDto>> GetUserPublicListsAsync(Guid targetUserId, Guid? currentUserId)
    {
        // el currentUserId sirve para calcular la propiedad IsFollowedByMe, si es null (no logueado)
        // sera falsa toda
        var query =
            from list in db.MangaLists
            join owner in db.Users on list.UserId equals owner.Id
            where list.IsPublic && list.UserId == targetUserId
            select new { list, owner };
        
        var results = await query
            .OrderByDescending(x => x.list.CreatedAt)
            .Select(x => new PublicListProjection(
                x.list, x.owner.Id, x.owner.DisplayName, x.owner.AvatarUrl,
                x.list.MangaListItems.Count, x.list.Followers.Count,
                currentUserId != null && x.list.Followers.Any(f => f.FollowerUserId == currentUserId)
            ))
            .ToListAsync();

        return results.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PublicMangaListDto>> GetFollowedListsAsync(Guid userId)
    {
        var query =
            from follow in db.ListFollows
            join list in db.MangaLists on follow.MangaListId equals list.Id
            join owner in db.Users on list.UserId equals owner.Id
            where follow.FollowerUserId == userId
            select new { list, owner };
        
        var results = await query
            .OrderByDescending(x => x.list.CreatedAt)
            .Select(x => new PublicListProjection(
                x.list, x.owner.Id, x.owner.DisplayName, x.owner.AvatarUrl,
                x.list.MangaListItems.Count, x.list.Followers.Count, true
            ))
            .ToListAsync();

        return results.Select(ToDto).ToList();
    }

    public async Task FollowAsync(Guid userId, Guid listId)
    {
        var list = await db.MangaLists.FirstOrDefaultAsync(l => l.Id == listId)
                   ?? throw new NotFoundAppException("No se encontró esa lista.");

        if (list.UserId == userId)
            throw new ForbiddenAppException("No podés seguir tu propia lista.");

        if (!list.IsPublic)
            throw new ForbiddenAppException("Esa lista es privada.");

        var alreadyFollowing = await db.ListFollows
            .AnyAsync(f => f.FollowerUserId == userId && f.MangaListId == listId);
        if (alreadyFollowing)
            throw new ConflictAppException("Ya seguís esta lista.");

        db.ListFollows.Add(new ListFollow
        {
            Id = Guid.NewGuid(),
            FollowerUserId = userId,
            MangaListId = listId,
            FollowedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid userId, Guid listId)
    {
        var follow = await db.ListFollows
            .FirstOrDefaultAsync(f => f.FollowerUserId == userId && f.MangaListId == listId);

        if (follow is null) return;

        db.ListFollows.Remove(follow);
        await db.SaveChangesAsync();
    }
    
    public async Task<PublicUserProfileDto> GetUserProfileAsync(Guid userId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new NotFoundAppException("No se encontró ese usuario.");

        return new PublicUserProfileDto(user.Id, user.DisplayName, user.AvatarUrl);
    }

    private static PublicMangaListDto ToDto(PublicListProjection p) => new(
        p.List.Id, p.List.Name, p.List.Description, p.ItemCount, p.FollowerCount,
        p.OwnerId, p.OwnerDisplayName, p.OwnerAvatarUrl, p.IsFollowedByMe, p.List.CreatedAt);


    private record PublicListProjection(
        MangaList List, 
        Guid OwnerId, 
        string OwnerDisplayName, 
        string? OwnerAvatarUrl,
        int ItemCount, 
        int FollowerCount, 
        bool IsFollowedByMe);
}
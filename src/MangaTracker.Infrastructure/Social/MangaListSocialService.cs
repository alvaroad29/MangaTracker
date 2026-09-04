using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Social;

public class MangaListSocialService(ApplicationDbContext db) : IMangaListSocialService
{
    private const int PageSize = 20;

    public async Task<IReadOnlyList<PublicMangaListDto>> SearchPublicListsAsync(Guid? currentUserId, string? search, int page)
    {
        // el id es para calcular la propiedad IsFollowedByMe, si es null (no logueado) sera falsa toda
        var query = BasePublicListQuery(currentUserId);

        if (!string.IsNullOrWhiteSpace(search)) // busca lista por nombre
            query = query.Where(x => x.List.Name.Contains(search));

        var results = await query
            .OrderByDescending(x => x.FollowerCount)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return results.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PublicMangaListDto>> GetUserPublicListsAsync(Guid targetUserId, Guid? currentUserId)
    {
        // el currentUserId sirve para calcular la propiedad IsFollowedByMe, si es null (no logueado)
        // sera falsa toda
        var query = BasePublicListQuery(currentUserId).Where(x => x.List.UserId == targetUserId); // las listas que coincidan con el targetId (vino como parametro)
        var results = await query.OrderByDescending(x => x.List.CreatedAt).ToListAsync();
        return results.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<PublicMangaListDto>> GetFollowedListsAsync(Guid userId)
    {
        var query =
            from follow in db.ListFollows
            join list in db.MangaLists on follow.MangaListId equals list.Id
            join owner in db.Users on list.UserId equals owner.Id
            where follow.FollowerUserId == userId
            select new PublicListProjection(
                list,
                owner.Id,
                owner.DisplayName,
                owner.AvatarUrl,
                list.MangaListItems.Count,
                list.Followers.Count,
                true // obviamente la sigo, es la lista de "seguidas"
            );

        var results = await query.OrderByDescending(x => x.List.CreatedAt).ToListAsync();
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

        if (follow is null) return; // idempotente, mismo criterio que RemoveItemAsync en Fase 5

        db.ListFollows.Remove(follow);
        await db.SaveChangesAsync();
    }

    private IQueryable<PublicListProjection> BasePublicListQuery(Guid? currentUserId) =>
        
        from list in db.MangaLists
        join owner in db.Users on list.UserId equals owner.Id
        where list.IsPublic
        select new PublicListProjection(
            list,
            owner.Id,
            owner.DisplayName,
            owner.AvatarUrl,
            list.MangaListItems.Count,
            list.Followers.Count,
            currentUserId != null && list.Followers.Any(f => f.FollowerUserId == currentUserId)
        );

    private static PublicMangaListDto ToDto(PublicListProjection p) => new(
        p.List.Id, p.List.Name, p.List.Description, p.ItemCount, p.FollowerCount,
        p.OwnerId, p.OwnerDisplayName, p.OwnerAvatarUrl, p.IsFollowedByMe, p.List.CreatedAt);

    // proyección intermedia — evita repetir la forma del "select new {...}" en cada método
    private record PublicListProjection(
        MangaList List, 
        Guid OwnerId, 
        string OwnerDisplayName, 
        string? OwnerAvatarUrl,
        int ItemCount, 
        int FollowerCount, 
        bool IsFollowedByMe);
}
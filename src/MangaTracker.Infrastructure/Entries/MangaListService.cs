using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.DTOs.Entries;
using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Entries;

public class MangaListService(ApplicationDbContext db) : IMangaListService
{
    public async Task<MangaListDto> CreateAsync(Guid userId, CreateMangaListRequest request)
    {
        var nameTaken = await db.MangaLists.AnyAsync(l => l.UserId == userId && l.Name == request.Name);
        if (nameTaken)
            throw new ConflictAppException("Ya tenés una lista con ese nombre.");

        var list = new MangaList
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        db.MangaLists.Add(list);
        await db.SaveChangesAsync();

        return ToDto(list, 0);
    }

    public async Task<IReadOnlyList<MangaListDto>> GetAllAsync(Guid userId)
    {
        var lists = await db.MangaLists
            .Where(l => l.UserId == userId)
            .Select(l => new { List = l, ItemCount = l.MangaListItems.Count })
            .OrderByDescending(x => x.List.CreatedAt)
            .ToListAsync();

        return lists.Select(x => ToDto(x.List, x.ItemCount)).ToList();
    }

    public async Task<MangaListDto> UpdateAsync(Guid userId, Guid listId, UpdateMangaListRequest request)
    {
        var list = await FindOwnedListAsync(userId, listId);

        var nameTaken = await db.MangaLists
            .AnyAsync(l => l.UserId == userId && l.Name == request.Name && l.Id != listId);
        if (nameTaken)
            throw new ConflictAppException("Ya tenés una lista con ese nombre.");

        list.Name = request.Name;
        list.Description = request.Description;
        list.IsPublic = request.IsPublic;

        await db.SaveChangesAsync();

        var itemCount = await db.MangaListItems.CountAsync(i => i.MangaListId == listId);
        return ToDto(list, itemCount);
    }

    public async Task DeleteAsync(Guid userId, Guid listId)
    {
        var list = await FindOwnedListAsync(userId, listId);
        db.MangaLists.Remove(list); // cascada configurada en Fase 1: borra MangaListItems y ListFollows asociados
        await db.SaveChangesAsync();
    }
    
    public async Task<IReadOnlyList<MangaListItemDto>> GetItemsAsync(Guid? viewerUserId, Guid listId)
    {
        var list = await db.MangaLists.FirstOrDefaultAsync(l => l.Id == listId)
                   ?? throw new NotFoundAppException("No se encontró esa lista.");

        var isOwner = viewerUserId.HasValue && list.UserId == viewerUserId.Value; // si es el dueño
        
        
        if (!isOwner && !list.IsPublic) // solo entra si no es el dueño y la lista es privada 
            throw new NotFoundAppException("No se encontró esa lista."); // mismo mensaje que "no existe": no delatamos que es privada

        var items = await db.MangaListItems
            .Include(i => i.UserMangaEntry)
            .ThenInclude(e => e.MangaSnapshot)
            .ThenInclude(s => s.MangaSnapshotGenres)
            .ThenInclude(sg => sg.Genre)
            .Where(i => i.MangaListId == listId)
            .OrderByDescending(i => i.AddedAt)
            .ToListAsync();

        return items.Select(ToItemDto).ToList();
    }

    // public async Task<IReadOnlyList<MangaListItemDto>> GetItemsAsync(Guid userId, Guid listId)
    // {
    //     await FindOwnedListAsync(userId, listId); // valida ownership antes de listar
    //
    //     var items = await db.MangaListItems
    //         .Include(i => i.UserMangaEntry)
    //             .ThenInclude(e => e.MangaSnapshot)
    //                 .ThenInclude(s => s.MangaSnapshotGenres)
    //                     .ThenInclude(sg => sg.Genre)
    //         .Where(i => i.MangaListId == listId)
    //         .OrderByDescending(i => i.AddedAt)
    //         .ToListAsync();
    //
    //     return items.Select(ToItemDto).ToList();
    // }

    public async Task AddItemAsync(Guid userId, Guid listId, Guid entryId)
    {
        await FindOwnedListAsync(userId, listId);

        var entryOwned = await db.UserMangaEntries.AnyAsync(e => e.Id == entryId && e.UserId == userId);
        if (!entryOwned)
            throw new NotFoundAppException("No se encontró ese manga en tu lista de progreso.");

        var alreadyInList = await db.MangaListItems
            .AnyAsync(i => i.MangaListId == listId && i.UserMangaEntryId == entryId);
        if (alreadyInList)
            throw new ConflictAppException("Ese manga ya está en esta lista.");

        db.MangaListItems.Add(new MangaListItem
        {
            Id = Guid.NewGuid(),
            MangaListId = listId,
            UserMangaEntryId = entryId,
            AddedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid userId, Guid listId, Guid entryId)
    {
        await FindOwnedListAsync(userId, listId);

        var item = await db.MangaListItems
            .FirstOrDefaultAsync(i => i.MangaListId == listId && i.UserMangaEntryId == entryId);

        if (item is null) return; // ya no estaba — no es un error, es idempotente

        db.MangaListItems.Remove(item);
        await db.SaveChangesAsync();
    }

    private async Task<MangaList> FindOwnedListAsync(Guid userId, Guid listId)
    {
        var list = await db.MangaLists.FirstOrDefaultAsync(l => l.Id == listId && l.UserId == userId);
        return list ?? throw new NotFoundAppException("No se encontró esa lista.");
    }

    private static MangaListDto ToDto(MangaList l, int itemCount) =>
        new(l.Id, l.Name, l.Description, l.IsPublic, itemCount, l.CreatedAt);

    private static MangaListItemDto ToItemDto(MangaListItem i)
    {
        var e = i.UserMangaEntry;
        var s = e.MangaSnapshot;

        var snapshotDto = new MangaSnapshotDto(
            s.Id, s.MalId, s.Title, s.TitleEnglish, s.CoverImageUrl, s.Type, s.PublicationStatus, s.Synopsis,
            s.MangaSnapshotGenres.Select(sg => new GenreDto(sg.Genre.Id, sg.Genre.Name)).ToList());

        var entryDto = new MangaEntryDto(
            e.Id, snapshotDto, e.Status, e.LastChapterRead, e.Score, e.Notes, e.StartedAt, e.FinishedAt, e.CreatedAt, e.UpdatedAt);

        return new MangaListItemDto(i.Id, entryDto, i.AddedAt);
    }
}
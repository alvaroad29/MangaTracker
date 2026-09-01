using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.DTOs.Entries;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Domain.Enums;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Entries;

public class UserMangaEntryService(ApplicationDbContext db, IMangaSnapshotService snapshotService) : IUserMangaEntryService
{
    public async Task<MangaEntryDto> AddAsync(Guid userId, int malId)
    {
        var snapshot = await snapshotService.GetOrCreateAsync(malId);

        var alreadyTracked = await db.UserMangaEntries
            .AnyAsync(e => e.UserId == userId && e.MangaSnapshotId == snapshot.Id);

        if (alreadyTracked)
            throw new ConflictAppException("Ya estás siguiendo el progreso de este manga.");

        var entry = new UserMangaEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MangaSnapshotId = snapshot.Id,
            Status = ReadingStatus.PlanToRead,
            LastChapterRead = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.UserMangaEntries.Add(entry);
        await db.SaveChangesAsync();

        return ToDto(entry, snapshot);
    }

    public async Task<IReadOnlyList<MangaEntryDto>> GetAllAsync(Guid userId, ReadingStatus? statusFilter)
    {
        var query = db.UserMangaEntries
            .Include(e => e.MangaSnapshot)
                .ThenInclude(s => s.MangaSnapshotGenres)
                    .ThenInclude(sg => sg.Genre)
            .Where(e => e.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(e => e.Status == statusFilter.Value);

        var entries = await query.OrderByDescending(e => e.UpdatedAt).ToListAsync();

        return entries.Select(e => ToDto(e, ToSnapshotDto(e.MangaSnapshot))).ToList();
    }

    public async Task<MangaEntryDto> UpdateAsync(Guid userId, Guid entryId, UpdateMangaEntryRequest request)
    {
        var entry = await FindOwnedEntryAsync(userId, entryId);

        entry.Status = request.Status;
        entry.LastChapterRead = request.LastChapterRead;
        entry.Score = request.Score;
        entry.Notes = request.Notes;
        entry.UpdatedAt = DateTime.UtcNow;

        if (request.Status == ReadingStatus.Reading && entry.StartedAt is null)
            entry.StartedAt = DateTime.UtcNow;

        if (request.Status == ReadingStatus.Completed && entry.FinishedAt is null)
            entry.FinishedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return ToDto(entry, ToSnapshotDto(entry.MangaSnapshot));
    }

    public async Task DeleteAsync(Guid userId, Guid entryId)
    {
        var entry = await FindOwnedEntryAsync(userId, entryId);
        db.UserMangaEntries.Remove(entry);
        await db.SaveChangesAsync();
    }

    private async Task<UserMangaEntry> FindOwnedEntryAsync(Guid userId, Guid entryId)
    {
        var entry = await db.UserMangaEntries
            .Include(e => e.MangaSnapshot)
                .ThenInclude(s => s.MangaSnapshotGenres)
                    .ThenInclude(sg => sg.Genre)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);

        return entry ?? throw new NotFoundAppException("No se encontró ese manga en tu lista.");
    }

    private static MangaSnapshotDto ToSnapshotDto(MangaSnapshot s) => new(
        s.Id, s.MalId, s.Title, s.TitleEnglish, s.CoverImageUrl, s.Type, s.PublicationStatus, s.Synopsis,
        s.MangaSnapshotGenres.Select(sg => new GenreDto(sg.Genre.Id, sg.Genre.Name)).ToList());

    private static MangaEntryDto ToDto(UserMangaEntry e, MangaSnapshotDto snapshot) => new(
        e.Id, snapshot, e.Status, e.LastChapterRead, e.Score, e.Notes, e.StartedAt, e.FinishedAt, e.CreatedAt, e.UpdatedAt);
}
using System.Xml.Linq;
using MangaTracker.Application.DTOs.Backup;
using MangaTracker.Application.DTOs.Entries;
using MangaTracker.Application.DTOs.Lists;
using MangaTracker.Application.Exceptions;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Enums;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Backup;

public class BackupService(
    ApplicationDbContext db,
    IUserMangaEntryService entryService,
    IMangaListService listService) : IBackupService
{
    public async Task<BackupExportDto> BuildJsonExportAsync(Guid userId)
    {
        var entries = await db.UserMangaEntries
            .Include(e => e.MangaSnapshot)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var entryDtos = entries.Select(e => new BackupEntryDto(
            e.MangaSnapshot.MalId,
            e.MangaSnapshot.Title,
            e.MangaSnapshot.CoverImageUrl,
            e.MangaSnapshot.Type,
            e.Status, e.LastChapterRead, e.Score, e.Notes, e.StartedAt, e.FinishedAt)).ToList();

        var lists = await db.MangaLists
            .Include(l => l.MangaListItems)
                .ThenInclude(i => i.UserMangaEntry)
                    .ThenInclude(e => e.MangaSnapshot)
            .Where(l => l.UserId == userId)
            .ToListAsync();

        var listDtos = lists.Select(l => new BackupListDto(
            l.Name, 
            l.Description, 
            l.IsPublic,
            l.MangaListItems.Select(i => new BackupListMangaDto(
                i.UserMangaEntry.MangaSnapshot.MalId,
                i.UserMangaEntry.MangaSnapshot.Title
            )).ToList()
        )).ToList();

        return new BackupExportDto(DateTime.UtcNow, entryDtos, listDtos);
    }

    public async Task<string> BuildMalXmlExportAsync(Guid userId)
    {
        var entries = await db.UserMangaEntries
            .Include(e => e.MangaSnapshot)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var statusCounts = entries.GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count());

        var myinfo = new XElement("myinfo",
            new XElement("user_export_type", 2), // 2 = manga (1 sería anime)
            new XElement("user_total_manga", entries.Count),
            new XElement("user_total_reading", statusCounts.GetValueOrDefault(ReadingStatus.Reading)),
            new XElement("user_total_completed", statusCounts.GetValueOrDefault(ReadingStatus.Completed)),
            new XElement("user_total_onhold", statusCounts.GetValueOrDefault(ReadingStatus.OnHold)),
            new XElement("user_total_dropped", statusCounts.GetValueOrDefault(ReadingStatus.Dropped)),
            new XElement("user_total_plantoread", statusCounts.GetValueOrDefault(ReadingStatus.PlanToRead)));

        var mangaElements = entries.Select(e => new XElement("manga",
            new XElement("manga_mangadb_id", e.MangaSnapshot.MalId),
            new XElement("manga_title", new XCData(e.MangaSnapshot.Title)),
            new XElement("manga_volumes", 0),
            new XElement("manga_chapters", 0),
            new XElement("my_id", 0),
            new XElement("my_read_volumes", 0),
            new XElement("my_read_chapters", (int)e.LastChapterRead),
            new XElement("my_start_date", e.StartedAt?.ToString("yyyy-MM-dd") ?? "0000-00-00"),
            new XElement("my_finish_date", e.FinishedAt?.ToString("yyyy-MM-dd") ?? "0000-00-00"),
            new XElement("my_score", e.Score ?? 0),
            new XElement("my_status", MapStatusToMal(e.Status)),
            new XElement("my_comments", new XCData(e.Notes ?? string.Empty)),
            new XElement("my_tags", new XCData(string.Empty)),
            new XElement("update_on_import", 1)));

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("myanimelist", myinfo, mangaElements));

        return doc.ToString();
    }

    public async Task<ImportResultDto> ImportJsonAsync(Guid userId, BackupExportDto backup)
    {
        // mapa MalId -> Id interno del entry, lo vamos llenando a medida que procesamos,
        // lo necesitamos después para reconstruir qué manga va en qué lista
        var entryMap = new Dictionary<int, Guid>();
        int entriesImported = 0, entriesSkipped = 0, listsImported = 0, listsSkipped = 0;

        foreach (var entry in backup.Entries)
        {
            try
            {
                var created = await entryService.AddFromBackupAsync(userId, entry); // ya trae todos los datos, sin red externa
                entryMap[entry.MalId] = created.Id;
                entriesImported++;
            }
            catch (ConflictAppException)
            {
                var existing = await entryService.GetByMalIdAsync(userId, entry.MalId);
                if (existing is not null) entryMap[entry.MalId] = existing.Id;
                entriesSkipped++;
            }
        }

        foreach (var list in backup.Lists)
        {
            try
            {
                // CreateAsync ya valida nombre único y tira ConflictAppException si ya existe —
                // de nuevo, exactamente el comportamiento que queríamos para "no duplicar"
                var created = await listService.CreateAsync(userId, new CreateMangaListRequest(list.Name, list.Description, list.IsPublic));

                foreach (var manga in list.Mangas)
                {
                    if (!entryMap.TryGetValue(manga.MalId, out var entryId)) continue; // no se pudo resolver ese manga, lo salteamos

                    try { await listService.AddItemAsync(userId, created.Id, entryId); }
                    catch (ConflictAppException) { /* ya estaba en la lista, no pasa nada */ }
                }

                listsImported++;
            }
            catch (ConflictAppException)
            {
                listsSkipped++; // ya tenías una lista con ese nombre — no la tocamos
            }
        }

        return new ImportResultDto(entriesImported, entriesSkipped, listsImported, listsSkipped);
    }

    private static string MapStatusToMal(ReadingStatus status) => status switch
    {
        ReadingStatus.Reading => "Reading",
        ReadingStatus.Completed => "Completed",
        ReadingStatus.OnHold => "On-Hold",
        ReadingStatus.Dropped => "Dropped",
        ReadingStatus.PlanToRead => "Plan to Read",
        _ => "Plan to Read"
    };
}
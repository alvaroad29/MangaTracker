namespace MangaTracker.Application.DTOs.Backup;

public record ImportResultDto(int EntriesImported, int EntriesSkipped, int ListsImported, int ListsSkipped);
namespace MangaTracker.Application.DTOs.Backup;

public record BackupExportDto(DateTime ExportedAt, List<BackupEntryDto> Entries, List<BackupListDto> Lists);
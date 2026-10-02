using MangaTracker.Application.DTOs.Backup;

namespace MangaTracker.Application.Interfaces;

public interface IBackupService
{
    Task<BackupExportDto> BuildJsonExportAsync(Guid userId);
    Task<string> BuildMalXmlExportAsync(Guid userId);
    Task<ImportResultDto> ImportJsonAsync(Guid userId, BackupExportDto backup);
}
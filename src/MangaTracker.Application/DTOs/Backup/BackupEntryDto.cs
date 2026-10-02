using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Backup;

// Identificado por MalId, no por el Guid interno — así el backup es portable
// entre entornos (si reimportás en otra base, los Guid internos no coinciden, pero MalId sí)
public record BackupEntryDto(
    int MalId,
    string Title,            
    string CoverImageUrl,    // URL de Cloudinary, no depende de Tenrai
    MangaType Type,          
    ReadingStatus Status,
    decimal LastChapterRead,
    int? Score,
    string? Notes,
    DateTime? StartedAt,
    DateTime? FinishedAt);
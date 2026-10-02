namespace MangaTracker.Application.DTOs.Backup;

public record BackupListMangaDto(int MalId, string Title);
public record BackupListDto(string Name, string? Description, bool IsPublic, List<BackupListMangaDto> Mangas);


namespace MangaTracker.Application.DTOs.Lists;

public record UpdateMangaListRequest(string Name, string? Description, bool IsPublic);
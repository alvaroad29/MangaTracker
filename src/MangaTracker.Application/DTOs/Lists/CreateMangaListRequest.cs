namespace MangaTracker.Application.DTOs.Lists;

public record CreateMangaListRequest(string Name, string? Description, bool IsPublic);
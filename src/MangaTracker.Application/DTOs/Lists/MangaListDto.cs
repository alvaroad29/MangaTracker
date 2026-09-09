namespace MangaTracker.Application.DTOs.Lists;

// public record MangaListDto(Guid Id, string Name, string? Description, bool IsPublic, int ItemCount, DateTime CreatedAt);



public record MangaListDto(Guid Id, string Name, string? Description, bool IsPublic, int ItemCount, Guid OwnerId, DateTime CreatedAt);
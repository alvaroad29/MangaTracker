using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Entries;

public record UpdateMangaEntryRequest(
    ReadingStatus Status,
    decimal LastChapterRead,
    int? Score,
    string? Notes,
    DateTime? StartedAt,
    DateTime? FinishedAt);
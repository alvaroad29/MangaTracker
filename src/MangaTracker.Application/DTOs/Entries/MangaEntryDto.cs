using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Entries;

public record MangaEntryDto(
    Guid Id,
    MangaSnapshotDto Manga,
    ReadingStatus Status,
    decimal LastChapterRead,
    int? Score,
    string? Notes,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
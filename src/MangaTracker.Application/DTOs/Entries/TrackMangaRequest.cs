using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Entries;

public record TrackMangaRequest(
    int MalId,
    ReadingStatus Status
    );
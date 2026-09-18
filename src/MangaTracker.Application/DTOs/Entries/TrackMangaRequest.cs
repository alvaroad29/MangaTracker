using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Entries;

public record AddMangaToListRequest(
    int MalId,
    ReadingStatus Status
    );
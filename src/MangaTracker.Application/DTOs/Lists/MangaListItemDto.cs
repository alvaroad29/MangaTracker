using MangaTracker.Application.DTOs.Entries;

namespace MangaTracker.Application.DTOs.Lists;

public record MangaListItemDto(Guid Id, MangaEntryDto Entry, DateTime AddedAt);
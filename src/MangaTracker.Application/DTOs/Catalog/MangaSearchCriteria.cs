using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Catalog;

public record MangaSearchCriteria(
    string SearchQuery,
    MangaType? Type,
    PublicationStatus? Status,
    IReadOnlyList<string> Genres);
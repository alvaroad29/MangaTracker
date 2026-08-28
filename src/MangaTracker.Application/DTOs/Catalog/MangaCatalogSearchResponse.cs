namespace MangaTracker.Application.DTOs.Catalog;

public record MangaCatalogSearchResponse(
    IReadOnlyList<MangaCatalogSearchResultDto> Items,
    int CurrentPage,
    bool HasNextPage);
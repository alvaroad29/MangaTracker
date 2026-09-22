namespace MangaTracker.Application.DTOs.Lists;


public record PublicListSearchResponse(
    IReadOnlyList<PublicMangaListDto> Items,
    int CurrentPage,
    bool HasNextPage,
    int LastVisiblePage,
    int TotalItemsCount);
using MangaTracker.Application.DTOs.Catalog;

namespace MangaTracker.Application.DTOs.AiSearch;

public record AiSearchResponse(MangaSearchCriteria Interpretation, MangaCatalogSearchResponse Results);
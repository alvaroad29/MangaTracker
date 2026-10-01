using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Domain.Enums;
using MangaTracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Tenrai;
using MangaType = MangaTracker.Domain.Enums.MangaType;
using TenraiMangaType = Tenrai.MangaType; // alias: choca de nombre con nuestro Domain.Enums.MangaType


namespace MangaTracker.Infrastructure.Catalog;

public class TenraiCatalogService(ITenrai tenrai, CatalogMapper mapper, IMemoryCache cache) : IMangaCatalogService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(2);

    public async Task<MangaCatalogSearchResponse> SearchMangaAsync(string query, int page = 1)
    {
        var cacheKey = $"manga-search:{query.Trim().ToLowerInvariant()}:{page}";

        if (cache.TryGetValue(cacheKey, out MangaCatalogSearchResponse? cached) && cached is not null)
            return cached;
        
        var config = new MangaSearchConfig
        {
            // tamaño por defecto = 25
            Query = query,
            Page = page
        };

        var result = await tenrai.SearchMangaAsync( config );

        var response = new MangaCatalogSearchResponse(
            result.Data.Select(mapper.ToSearchResultDto).ToList(),
            page, // numero de pagina
            result.Pagination?.HasNextPage ?? false, // hay siguiente pagina
            result.Pagination?.LastVisiblePage, // ultima pagina
            result.Pagination?.Items.Total // cantidad total de resultados
            );

        cache.Set(cacheKey, response, CacheTtl);
        return response;
    }

    public async Task<MangaCatalogDetailDto?> GetMangaByMalIdAsync(int malId)
    {
        var cacheKey = $"manga-detail:{malId}";

        if (cache.TryGetValue(cacheKey, out MangaCatalogDetailDto? cached) && cached is not null)
            return cached;

        var result = await tenrai.GetMangaAsync(malId);
        if (result?.Data is null) return null;

        var detail = mapper.ToDetailDto(result.Data);
        cache.Set(cacheKey, detail, CacheTtl);
        return detail;
    }
    
    
    public async Task<MangaCatalogSearchResponse> SearchAdvancedAsync(MangaSearchCriteria criteria, int page = 1)
    {
        var config = new MangaSearchConfig
        {
            Query = criteria.SearchQuery,
            Page = page,
            OrderBy = MangaSearchOrderBy.Score,      // resultados de mayor puntaje primero — más relevantes
            SortDirection = SortDirection.Descending
        };

        if (criteria.Type is not null) // tipo manga
            config.Type = MapType(criteria.Type.Value);

        if (criteria.Status is not null)
            config.Status = MapStatus(criteria.Status.Value);

        foreach (var genreName in criteria.Genres)
        {
            if (Enum.TryParse<MangaGenreSearch>(genreName, out var genre))
                config.Genres.Add(genre); // si la IA devolvió un nombre que no matchea ningún género real, lo ignoramos, sin romper
        }

        var result = await tenrai.SearchMangaAsync(config);

        return new MangaCatalogSearchResponse(
            result.Data.Select(mapper.ToSearchResultDto).ToList(),
            page,
            result.Pagination?.HasNextPage ?? false,
            result.Pagination?.LastVisiblePage ?? page,
            result.Pagination?.Items?.Total ?? result.Data.Count);
    }

    private static TenraiMangaType MapType(MangaType type) => type switch
    {
        MangaType.Manga => TenraiMangaType.Manga,
        MangaType.Manhwa => TenraiMangaType.Manhwa,
        MangaType.Manhua => TenraiMangaType.Manhua,
        MangaType.LightNovel => TenraiMangaType.Novel,
        MangaType.OneShot => TenraiMangaType.OneShot,
        MangaType.Doujinshi => TenraiMangaType.Doujinshi,
        _ => TenraiMangaType.EveryType
    };

    private static PublishingStatus MapStatus(PublicationStatus status) => status switch
    {
        PublicationStatus.Publishing => PublishingStatus.Publishing,
        PublicationStatus.Finished => PublishingStatus.Complete, // Tenrai lo llama "Complete", nosotros "Finished"
        PublicationStatus.Hiatus => PublishingStatus.Hiatus,
        PublicationStatus.Discontinued => PublishingStatus.Discontinued,
        _ => PublishingStatus.EveryStatus
    };
}
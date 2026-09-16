using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Tenrai;

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
}
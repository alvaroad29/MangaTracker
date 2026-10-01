using MangaTracker.Application.DTOs.Catalog;

namespace MangaTracker.Application.Interfaces;

public interface IMangaCatalogService
{
    Task<MangaCatalogSearchResponse> SearchMangaAsync(string query, int page = 1); // traer todos los mangas
    Task<MangaCatalogSearchResponse> SearchAdvancedAsync(MangaSearchCriteria criteria, int page = 1);
    Task<MangaCatalogDetailDto?> GetMangaByMalIdAsync(int malId); // manga en especifico
}
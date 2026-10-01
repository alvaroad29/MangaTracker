using System.Text.Json;
using MangaTracker.Application.DTOs.AiSearch;
using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.Interfaces;

namespace MangaTracker.Infrastructure.AiSearch;

public class AiSearchService(IAiSearchTranslationService translator, IMangaCatalogService catalogService) : IAiSearchService
{
    public async Task<AiSearchResponse> SearchAsync(string description)
    {
        var criteria = await translator.TranslateAsync(description);
        Console.WriteLine("===================");
        Console.WriteLine(JsonSerializer.Serialize(criteria, new JsonSerializerOptions { WriteIndented = true }));
        
        var results = await catalogService.SearchAdvancedAsync(criteria, page: 1);
        return new AiSearchResponse(criteria, results);
    }
}
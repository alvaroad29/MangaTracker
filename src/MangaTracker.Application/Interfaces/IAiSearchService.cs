using MangaTracker.Application.DTOs.AiSearch;

namespace MangaTracker.Application.Interfaces;

public interface IAiSearchService
{
    Task<AiSearchResponse> SearchAsync(string description);
}
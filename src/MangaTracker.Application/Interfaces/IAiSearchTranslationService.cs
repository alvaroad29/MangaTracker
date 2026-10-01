using MangaTracker.Application.DTOs.AiSearch;
using MangaTracker.Application.DTOs.Catalog;

namespace MangaTracker.Application.Interfaces;

public interface IAiSearchTranslationService
{
    Task<MangaSearchCriteria> TranslateAsync(string naturalLanguageDescription);
}
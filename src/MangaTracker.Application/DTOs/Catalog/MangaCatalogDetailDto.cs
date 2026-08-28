using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Catalog;

public record MangaCatalogDetailDto(
    int MalId,
    string Title,
    string? TitleEnglish,
    string CoverImageUrl,
    MangaType Type,
    PublicationStatus? Status,
    string? Synopsis,
    IReadOnlyList<GenreDto> Genres);
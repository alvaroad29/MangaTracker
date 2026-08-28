using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.DTOs.Catalog;

public record MangaCatalogSearchResultDto(
    int MalId,
    string Title,
    string? TitleEnglish,
    string CoverImageUrl,
    MangaType Type,
    PublicationStatus? Status);
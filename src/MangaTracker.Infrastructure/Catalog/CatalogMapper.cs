using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Domain.Enums;
using Riok.Mapperly.Abstractions;
using Tenrai;
using MangaType = MangaTracker.Domain.Enums.MangaType; // enum propio


namespace MangaTracker.Infrastructure.Catalog;

[Mapper]
public partial class CatalogMapper
{
    public MangaCatalogSearchResultDto ToSearchResultDto(Manga manga) => new(
        (int)manga.MalId,
        manga.Title,
        manga.TitleEnglish,
        MapCoverImage(manga),
        MapMangaType(manga.Type),
        MapStatus(manga.Status)
    );

    public MangaCatalogDetailDto ToDetailDto(Manga manga) => new(
        (int)manga.MalId,
        manga.Title,
        manga.TitleEnglish,
        MapCoverImage(manga),
        MapMangaType(manga.Type),
        MapStatus(manga.Status),
        manga.Synopsis,
        manga.Genres?.Select(MapGenre).ToList() ?? []
    );


    private MangaType MapMangaType(string type) => type.ToLowerInvariant() switch
    {
        "manga" => MangaType.Manga,
        "manhwa" => MangaType.Manhwa,
        "manhua" => MangaType.Manhua,
        "novel" or "light novel" => MangaType.LightNovel,
        "one-shot" => MangaType.OneShot,
        "doujinshi" => MangaType.Doujinshi,
        _ => MangaType.Manga // fallback razonable si MAL agrega un tipo nuevo que no contemplamos
    };

    private PublicationStatus? MapStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "publishing" => PublicationStatus.Publishing,
        "finished" => PublicationStatus.Finished,
        "on hiatus" => PublicationStatus.Hiatus,
        "discontinued" => PublicationStatus.Discontinued,
        _ => null
    };

    private static string MapCoverImage(Manga manga) =>
        manga.Images?.JPG?.ImageUrl
        ?? manga.Images?.WebP?.ImageUrl
        ?? string.Empty;

    private partial GenreDto MapGenre(MalUrl genre);
}
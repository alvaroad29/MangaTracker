using MangaTracker.Domain.Enums;

namespace MangaTracker.Domain.Entities;


public class MangaSnapshot
{
    public Guid Id { get; set; }
    public int MalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleEnglish { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public MangaType Type { get; set; }
    public PublicationStatus? PublicationStatus { get; set; }
    public string? Synopsis { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<MangaSnapshotGenre> MangaSnapshotGenres { get; set; } = [];
    public ICollection<UserMangaEntry> UserMangaEntries { get; set; } = [];
}
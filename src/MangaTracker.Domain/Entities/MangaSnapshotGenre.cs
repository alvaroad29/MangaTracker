namespace MangaTracker.Domain.Entities;

public class MangaSnapshotGenre
{
    public Guid MangaSnapshotId { get; set; }
    public MangaSnapshot MangaSnapshot { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}
namespace MangaTracker.Domain.Entities;

public class Genre
{
    public int Id { get; set; } // mismo id que usa Jikan/MAL para el género
    public string Name { get; set; } = string.Empty;

    public ICollection<MangaSnapshotGenre> MangaSnapshotGenres { get; set; } = [];
}
namespace MangaTracker.Domain.Entities;

public class MangaListItem
{
    public Guid Id { get; set; }

    public Guid MangaListId { get; set; }
    public MangaList MangaList { get; set; } = null!;

    public Guid UserMangaEntryId { get; set; }
    public UserMangaEntry UserMangaEntry { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}
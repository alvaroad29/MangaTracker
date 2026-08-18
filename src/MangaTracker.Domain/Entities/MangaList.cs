namespace MangaTracker.Domain.Entities;

public class MangaList
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<MangaListItem> MangaListItems { get; set; } = [];
    public ICollection<ListFollow> Followers { get; set; } = [];
}
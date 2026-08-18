namespace MangaTracker.Domain.Entities;

public class ListFollow
{
    public Guid Id { get; set; }
    public Guid FollowerUserId { get; set; }

    public Guid MangaListId { get; set; }
    public MangaList MangaList { get; set; } = null!;

    public DateTime FollowedAt { get; set; }
}
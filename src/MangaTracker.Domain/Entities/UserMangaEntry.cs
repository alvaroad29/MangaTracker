using MangaTracker.Domain.Enums;

namespace MangaTracker.Domain.Entities;

public class UserMangaEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK escalar a ApplicationUser (vive en Infrastructure)

    public Guid MangaSnapshotId { get; set; }
    public MangaSnapshot MangaSnapshot { get; set; } = null!;

    public ReadingStatus Status { get; set; }
    public decimal LastChapterRead { get; set; }
    public int? Score { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<MangaListItem> MangaListItems { get; set; } = [];
}
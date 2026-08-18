using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class UserMangaEntryConfiguration : IEntityTypeConfiguration<UserMangaEntry>
{
    public void Configure(EntityTypeBuilder<UserMangaEntry> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.MangaSnapshotId }).IsUnique();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.LastChapterRead).HasColumnType("decimal(8,1)");
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne(x => x.MangaSnapshot)
            .WithMany(x => x.UserMangaEntries)
            .HasForeignKey(x => x.MangaSnapshotId)
            .OnDelete(DeleteBehavior.Restrict); // no borrar el snapshot en cascada por error
    }
}
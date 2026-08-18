using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class MangaSnapshotGenreConfiguration : IEntityTypeConfiguration<MangaSnapshotGenre>
{
    public void Configure(EntityTypeBuilder<MangaSnapshotGenre> builder)
    {
        builder.HasKey(x => new { x.MangaSnapshotId, x.GenreId });

        builder.HasOne(x => x.MangaSnapshot)
            .WithMany(x => x.MangaSnapshotGenres)
            .HasForeignKey(x => x.MangaSnapshotId);

        builder.HasOne(x => x.Genre)
            .WithMany(x => x.MangaSnapshotGenres)
            .HasForeignKey(x => x.GenreId);
    }
}
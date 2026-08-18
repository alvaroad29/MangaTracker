using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;


public class MangaSnapshotConfiguration : IEntityTypeConfiguration<MangaSnapshot>
{
    public void Configure(EntityTypeBuilder<MangaSnapshot> builder)
    {
        builder.HasIndex(x => x.MalId).IsUnique();

        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TitleEnglish).HasMaxLength(500);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.PublicationStatus).HasConversion<string>().HasMaxLength(50);
    }
}
using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class MangaListConfiguration : IEntityTypeConfiguration<MangaList>
{
    public void Configure(EntityTypeBuilder<MangaList> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
    }
}
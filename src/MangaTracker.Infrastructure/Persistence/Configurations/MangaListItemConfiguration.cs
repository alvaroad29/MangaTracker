using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class MangaListItemConfiguration : IEntityTypeConfiguration<MangaListItem>
{
    public void Configure(EntityTypeBuilder<MangaListItem> builder)
    {
        builder.HasIndex(x => new { x.MangaListId, x.UserMangaEntryId }).IsUnique();

        builder.HasOne(x => x.MangaList)
            .WithMany(x => x.MangaListItems)
            .HasForeignKey(x => x.MangaListId)
            .OnDelete(DeleteBehavior.Cascade); // borrar la lista borra sus items

        builder.HasOne(x => x.UserMangaEntry)
            .WithMany(x => x.MangaListItems)
            .HasForeignKey(x => x.UserMangaEntryId)
            .OnDelete(DeleteBehavior.Cascade); // borrar el progreso borra su membership en listas
    }
}
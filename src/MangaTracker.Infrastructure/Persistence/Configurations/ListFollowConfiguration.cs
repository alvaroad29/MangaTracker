using MangaTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class ListFollowConfiguration : IEntityTypeConfiguration<ListFollow>
{
    public void Configure(EntityTypeBuilder<ListFollow> builder)
    {
        builder.HasIndex(x => new { x.FollowerUserId, x.MangaListId }).IsUnique();

        builder.HasOne(x => x.MangaList)
            .WithMany(x => x.Followers)
            .HasForeignKey(x => x.MangaListId)
            .OnDelete(DeleteBehavior.Cascade); // borrar la lista borra sus seguidores
    }
}
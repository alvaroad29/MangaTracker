using MangaTracker.Domain.Entities;
using MangaTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<
        ApplicationUser, 
        IdentityRole<Guid>, 
        Guid
    >(options)
{
    public DbSet<MangaSnapshot> MangaSnapshots => Set<MangaSnapshot>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MangaSnapshotGenre> MangaSnapshotGenres => Set<MangaSnapshotGenre>();
    public DbSet<UserMangaEntry> UserMangaEntries => Set<UserMangaEntry>();
    public DbSet<MangaList> MangaLists => Set<MangaList>();
    public DbSet<MangaListItem> MangaListItems => Set<MangaListItem>();
    public DbSet<ListFollow> ListFollows => Set<ListFollow>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // configura las tablas de Identity

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
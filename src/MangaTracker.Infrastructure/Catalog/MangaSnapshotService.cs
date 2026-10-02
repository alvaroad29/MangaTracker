using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Entities;
using MangaTracker.Domain.Enums;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Catalog;

public class MangaSnapshotService(
    ApplicationDbContext db,
    IMangaCatalogService catalogService,
    IImageStorageService imageStorage) : IMangaSnapshotService
{
    public async Task<MangaSnapshotDto> GetOrCreateAsync(int malId)
    {
        var existing = await FindExistingAsync(malId);
        if (existing is not null) // si existe lo devuelve
            return ToDto(existing);

        var created = await CreateSnapshotAsync(malId);
        return ToDto(created);
    }

    private async Task<MangaSnapshot?> FindExistingAsync(int malId) // si el manga y los generos estan
    {
        return await db.MangaSnapshots
            .Include(s => s.MangaSnapshotGenres)
            .ThenInclude(sg => sg.Genre)
            .FirstOrDefaultAsync(s => s.MalId == malId);
    }

    private async Task<MangaSnapshot> CreateSnapshotAsync(int malId)
    {
        var catalogDetail = await catalogService.GetMangaByMalIdAsync(malId)
            ?? throw new KeyNotFoundException($"No se encontró el manga con MalId {malId} en el catálogo.");

        var coverUrl = await imageStorage.UploadFromUrlAsync(catalogDetail.CoverImageUrl, $"manga-{malId}");
        var genres = await GetOrCreateGenresAsync(catalogDetail.Genres); // crea o trae los generos

        var snapshot = new MangaSnapshot
        {
            Id = Guid.NewGuid(),
            MalId = catalogDetail.MalId,
            Title = catalogDetail.Title,
            TitleEnglish = catalogDetail.TitleEnglish,
            CoverImageUrl = coverUrl,
            Type = catalogDetail.Type,
            PublicationStatus = catalogDetail.Status,
            Synopsis = catalogDetail.Synopsis,
            CreatedAt = DateTime.UtcNow,
            MangaSnapshotGenres = genres.Select(g => new MangaSnapshotGenre { Genre = g }).ToList()
        };

        db.MangaSnapshots.Add(snapshot);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Carrera: otro usuario creó el mismo snapshot entre nuestro chequeo inicial y este SaveChanges.
            // El índice único de MalId frenó el insert duplicado a nivel DB — reusamos el que ganó la carrera.
            
            var concurrent = await FindExistingAsync(malId);
            if (concurrent is not null)
                return concurrent; // otro proceso ganó la carrera, reusamos lo que insertó

            throw; // no fue por la carrera esperada — es un error real, no lo tapamos
        }

        return snapshot;
    }
    
    public async Task<MangaSnapshotDto> GetOrCreateFromKnownDataAsync(int malId, string title, string coverImageUrl, MangaType type)
    {
        var existing = await FindExistingAsync(malId);
        if (existing is not null)
            return ToDto(existing);

        // Sin llamar a Tenrai ni a Cloudinary: usamos la portada tal cual viene del backup
        // (ya es una URL de TU Cloudinary de una vez que se creó antes, no de Tenrai)
        var snapshot = new MangaSnapshot
        {
            Id = Guid.NewGuid(),
            MalId = malId,
            Title = title,
            CoverImageUrl = coverImageUrl,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            MangaSnapshotGenres = [] // sin géneros — el backup no los guarda, es un dato secundario que se puede perder
        };

        db.MangaSnapshots.Add(snapshot);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var concurrent = await FindExistingAsync(malId);
            if (concurrent is not null) return ToDto(concurrent);
            throw;
        }

        return ToDto(snapshot);
    }

    private async Task<List<Genre>> GetOrCreateGenresAsync(IReadOnlyList<GenreDto> genreDtos)
    {
        var malIds = genreDtos.Select(g => g.MalId).ToList();

        var existingGenres = await db.Genres
            .Where(g => malIds.Contains(g.Id))
            .ToListAsync();

        var missingGenres = genreDtos
            .Where(dto => existingGenres.All(g => g.Id != dto.MalId))
            .Select(dto => new Genre { Id = dto.MalId, Name = dto.Name })
            .ToList();

        if (missingGenres.Count > 0) // agrega si faltan generos
        {
            db.Genres.AddRange(missingGenres);
            await db.SaveChangesAsync(); // los géneros se guardan aparte, antes del snapshot que los referencia
        }

        return [.. existingGenres, .. missingGenres];
    }

    private static MangaSnapshotDto ToDto(MangaSnapshot snapshot) => new(
        snapshot.Id,
        snapshot.MalId,
        snapshot.Title,
        snapshot.TitleEnglish,
        snapshot.CoverImageUrl,
        snapshot.Type,
        snapshot.PublicationStatus,
        snapshot.Synopsis,
        snapshot.MangaSnapshotGenres.Select(sg => new GenreDto(sg.Genre.Id, sg.Genre.Name)).ToList()
    );
}
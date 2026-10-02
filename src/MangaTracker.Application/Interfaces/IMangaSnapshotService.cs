using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Domain.Enums;

namespace MangaTracker.Application.Interfaces;

public interface IMangaSnapshotService
{
    /// <summary>
    /// Si ya existe un snapshot para ese MalId, lo devuelve tal cual.
    /// Si no existe, lo crea: trae los datos de Tenrai, sube la portada a Cloudinary, y persiste todoo.
    /// </summary>
    Task<MangaSnapshotDto> GetOrCreateAsync(int malId);
    
    // No llama a ningún proveedor externo — usa los datos que ya tiene, tal cual vienen de un backup
    Task<MangaSnapshotDto> GetOrCreateFromKnownDataAsync(int malId, string title, string coverImageUrl, MangaType type);
}
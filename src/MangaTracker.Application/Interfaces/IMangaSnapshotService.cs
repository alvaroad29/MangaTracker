using MangaTracker.Application.DTOs.Catalog;

namespace MangaTracker.Application.Interfaces;

public interface IMangaSnapshotService
{
    /// <summary>
    /// Si ya existe un snapshot para ese MalId, lo devuelve tal cual.
    /// Si no existe, lo crea: trae los datos de Tenrai, sube la portada a Cloudinary, y persiste todoo.
    /// </summary>
    Task<MangaSnapshotDto> GetOrCreateAsync(int malId);
}
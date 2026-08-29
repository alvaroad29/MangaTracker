namespace MangaTracker.Application.Interfaces;

public interface IImageStorageService
{
    /// <summary>Descarga la imagen de <paramref name="sourceUrl"/> y la sube a nuestro storage propio.</summary>
    /// <returns>La URL pública en nuestro storage.</returns>
    Task<string> UploadFromUrlAsync(string sourceUrl, string publicId);
}
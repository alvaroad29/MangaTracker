using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MangaTracker.Application.Interfaces;

namespace MangaTracker.Infrastructure.Storage;

public class CloudinaryImageStorageService(Cloudinary cloudinary) : IImageStorageService
{
    public async Task<string> UploadFromUrlAsync(string sourceUrl, string publicId)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(sourceUrl), // Cloudinary soporta subir directo desde una URL externa
            PublicId = publicId,
            Folder = "mangatracker/covers",
            Overwrite = false // si ya existe ese publicId, no lo pisa (evita re-subir de más)
        };

        var result = await cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Error subiendo imagen a Cloudinary: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
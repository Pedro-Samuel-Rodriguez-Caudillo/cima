using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace cima.Images;

/// <summary>
/// Servicio de almacenamiento de imágenes usando ABP BlobStoring.
/// Abstrae el proveedor de storage (Azure, AWS S3, FileSystem, etc.)
/// </summary>
public class AbpBlobImageStorageService : IImageStorageService, ITransientDependency
{
    private readonly IBlobContainer<ListingImageBlobContainer> _blobContainer;
    private readonly ILogger<AbpBlobImageStorageService> _logger;

    private const int ThumbnailMaxWidth = 400;
    private const int ThumbnailMaxHeight = 300;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public AbpBlobImageStorageService(
        IBlobContainer<ListingImageBlobContainer> blobContainer,
        ILogger<AbpBlobImageStorageService> logger)
    {
        _blobContainer = blobContainer;
        _logger = logger;
    }

    public async Task<UploadImageResult> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string folder = "listings")
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blobName = string.IsNullOrEmpty(folder) 
            ? uniqueFileName 
            : $"{folder}/{uniqueFileName}";

        // Guardar imagen original
        imageStream.Position = 0;
        await _blobContainer.SaveAsync(blobName, imageStream, overrideExisting: true);

        // Generar y guardar thumbnail
        var thumbnailBlobName = blobName.Replace(Path.GetExtension(blobName), "_thumb" + Path.GetExtension(blobName));
        imageStream.Position = 0;
        using var thumbnailStream = await GenerateThumbnailAsync(imageStream);
        await _blobContainer.SaveAsync(thumbnailBlobName, thumbnailStream, overrideExisting: true);

        _logger.LogInformation("Imagen subida: {BlobName} con thumbnail {ThumbnailBlobName}", blobName, thumbnailBlobName);

        return new UploadImageResult
        {
            Url = blobName,
            ThumbnailUrl = thumbnailBlobName
        };
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        try
        {
            await _blobContainer.DeleteAsync(imageUrl);
            
            // Intentar borrar thumbnail también
            var thumbnailUrl = imageUrl.Replace(Path.GetExtension(imageUrl), "_thumb" + Path.GetExtension(imageUrl));
            await _blobContainer.DeleteAsync(thumbnailUrl);

            _logger.LogInformation("Imagen eliminada: {ImageUrl}", imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al eliminar imagen: {ImageUrl}", imageUrl);
        }
    }

    public ImageValidationResult ValidateImage(string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ImageValidationResult.Invalid("EMPTY_FILENAME", "El nombre del archivo es requerido", ImageValidationSeverity.Error);
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !Array.Exists(AllowedExtensions, e => e == extension))
        {
            return ImageValidationResult.Invalid("INVALID_EXTENSION", $"Extensión no permitida: {extension}", ImageValidationSeverity.Error);
        }

        if (fileSize <= 0)
        {
            return ImageValidationResult.Invalid("EMPTY_FILE", "El archivo está vacío", ImageValidationSeverity.Error);
        }

        if (fileSize > MaxFileSize)
        {
            return ImageValidationResult.Invalid("FILE_TOO_LARGE", $"El archivo excede el tamaño máximo de {MaxFileSize / 1024 / 1024}MB", ImageValidationSeverity.Error);
        }

        return ImageValidationResult.Valid();
    }

    private async Task<Stream> GenerateThumbnailAsync(Stream originalImage)
    {
        originalImage.Position = 0;
        using var image = await Image.LoadAsync(originalImage);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(ThumbnailMaxWidth, ThumbnailMaxHeight),
            Mode = ResizeMode.Max
        }));

        var thumbnailStream = new MemoryStream();
        await image.SaveAsWebpAsync(thumbnailStream);
        thumbnailStream.Position = 0;

        return thumbnailStream;
    }
}

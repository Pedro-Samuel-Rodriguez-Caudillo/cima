using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace cima.Images;

/// <summary>
/// Implementación del servicio de almacenamiento de imágenes en disco local
/// NOTA: Esta es una implementación simple para desarrollo
/// En producción se recomienda usar Azure Blob Storage o AWS S3
/// </summary>
public class LocalImageStorageService : IImageStorageService, ITransientDependency
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<LocalImageStorageService> _logger;
    private const long MaxFileSize = ImageStorageHelper.DefaultMaxFileSize;
    private const int ThumbnailWidth = 480;

    public LocalImageStorageService(
        IHostEnvironment hostEnvironment,
        ILogger<LocalImageStorageService> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<UploadImageResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "listings")
    {
        ImageStorageHelper.ValidateFileName(fileName);
        ImageStorageHelper.ValidateReadable(imageStream);
        var extension = ImageStorageHelper.GetExtensionOrThrow(fileName);
        var supportsThumbnails = ImageStorageHelper.SupportsThumbnails(extension);

        // Generar nombre único
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_thumb{extension}";

        // Construir ruta completa (usando ContentRootPath)
        var uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "images", folder);

        // Crear directorio si no existe
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        var thumbPath = Path.Combine(uploadsFolder, thumbnailFileName);

        // Copiar a memoria para reutilizar en original y thumbnail
        await using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer);

        ImageStorageHelper.ValidateFileSize(buffer.Length, MaxFileSize);
        ImageStorageHelper.ValidateImageMagicBytes(buffer, extension);

        // Guardar archivo original
        try
        {
            buffer.Position = 0;
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await buffer.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar imagen {FileName}", fileName);
            throw new UserFriendlyException("Error al guardar la imagen");
        }

        // Crear thumbnail si el formato es soportado; de lo contrario usar la misma URL
        string thumbnailUrl;
        if (supportsThumbnails)
        {
            try
            {
                var thumbnailStream = await ImageStorageHelper.TryGenerateThumbnailAsync(
                    buffer,
                    extension,
                    ThumbnailWidth);

                if (thumbnailStream == null)
                {
                    thumbnailUrl = $"/images/{folder}/{uniqueFileName}";
                }
                else
                {
                    await using var thumbStream = new FileStream(thumbPath, FileMode.Create);
                    await thumbnailStream.CopyToAsync(thumbStream);
                    thumbnailUrl = $"/images/{folder}/{thumbnailFileName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al generar thumbnail para {FileName}", fileName);
                thumbnailUrl = $"/images/{folder}/{uniqueFileName}";
            }
        }
        else
        {
            thumbnailUrl = $"/images/{folder}/{uniqueFileName}";
        }

        var url = $"/images/{folder}/{uniqueFileName}";
        return new UploadImageResult
        {
            Url = url,
            ThumbnailUrl = thumbnailUrl
        };
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Task.CompletedTask;
        }

        // Construir ruta física
        var relativePath = imageUrl.TrimStart('/');
        var filePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relativePath);

        // Eliminar archivo si existe
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar imagen {ImageUrl}", imageUrl);
            }
        }

        return Task.CompletedTask;
    }

    public ImageValidationResult ValidateImage(string fileName, long fileSize)
    {
        // Validar tamaño
        if (fileSize > MaxFileSize)
        {
            return ImageValidationResult.Invalid(
                "ImageTooLarge",
                $"El archivo excede el tamaño máximo permitido ({MaxFileSize / (1024 * 1024)}MB)",
                ImageValidationSeverity.Error);
        }

        // Validar extensión
        var extension = Path.GetExtension(fileName);
        if (!ImageStorageHelper.IsExtensionAllowed(extension))
        {
            return ImageValidationResult.Invalid(
                "UnsupportedExtension",
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", ImageStorageHelper.AllowedExtensions)}",
                ImageValidationSeverity.Warning);
        }

        return ImageValidationResult.Valid();
    }

    public Task<Stream> GetImageStreamAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new UserFriendlyException("La URL de la imagen es requerida");
        }

        // Construir ruta física
        var relativePath = imageUrl.TrimStart('/');
        var filePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relativePath);

        if (!File.Exists(filePath))
        {
            throw new UserFriendlyException($"La imagen no existe: {imageUrl}");
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }


}

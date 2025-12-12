using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
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
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public LocalImageStorageService(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<UploadImageResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "listings")
    {
        // Validar nombre de archivo
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new UserFriendlyException("El nombre del archivo es requerido");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException(
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", AllowedExtensions)}"
            );
        }

        var supportsThumbnails = extension is ".jpg" or ".jpeg" or ".png" or ".webp";

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

        // Validar tamaño en servidor
        if (buffer.Length > MaxFileSize)
        {
            throw new UserFriendlyException($"El archivo excede el tamaño máximo de {MaxFileSize / (1024 * 1024)}MB");
        }

        // Validar formato de imagen mediante magic bytes
        buffer.Position = 0;
        ValidateImageMagicBytes(buffer, extension);

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
            throw new UserFriendlyException($"Error al guardar la imagen: {ex.Message}");
        }

        // Crear thumbnail si el formato es soportado; de lo contrario usar la misma URL
        string thumbnailUrl;
        if (supportsThumbnails)
        {
            try
            {
                buffer.Position = 0;
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(buffer);

                const int targetWidth = 480;
                if (image.Width <= targetWidth)
                {
                    thumbnailUrl = $"/images/{folder}/{uniqueFileName}";
                }
                else
                {
                    var targetHeight = (int)Math.Max(1, Math.Round(image.Height * (targetWidth / (double)image.Width)));

                    using var thumb = image.Clone(ctx => ctx.Resize(targetWidth, targetHeight));

                    IImageEncoder encoder = extension switch
                    {
                        ".png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed },
                        ".webp" => new WebpEncoder { Quality = 80 },
                        _ => new JpegEncoder { Quality = 80 }
                    };

                    await using var thumbStream = new FileStream(thumbPath, FileMode.Create);
                    await thumb.SaveAsync(thumbStream, encoder);
                    thumbnailUrl = $"/images/{folder}/{thumbnailFileName}";
                }
            }
            catch
            {
                // En caso de error al generar thumbnail, usar la imagen original
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
            catch
            {
                // Log error pero no fallar
                // En producción usar ILogger
            }
        }

        return Task.CompletedTask;
    }

    public bool ValidateImage(string fileName, long fileSize)
    {
        // Validar tamaño
        if (fileSize > MaxFileSize)
        {
            return false;
        }

        // Validar extensión
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Valida que el archivo sea realmente una imagen verificando los magic bytes
    /// </summary>
    private void ValidateImageMagicBytes(Stream stream, string expectedExtension)
    {
        if (stream.Length < 12)
        {
            throw new UserFriendlyException("El archivo es demasiado pequeño para ser una imagen válida");
        }

        var buffer = new byte[12];
        stream.Position = 0;
        var bytesRead = stream.Read(buffer, 0, 12);
        stream.Position = 0; // Resetear posición para uso posterior

        if (bytesRead < 4)
        {
            throw new UserFriendlyException("No se pudo leer el archivo correctamente");
        }

        // Validar magic bytes según el formato esperado
        var isValid = expectedExtension switch
        {
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38,
            ".webp" => bytesRead >= 12 && 
                      buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 && // RIFF
                      buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50, // WEBP
            _ => false
        };

        if (!isValid)
        {
            throw new UserFriendlyException(
                $"El archivo no es una imagen {expectedExtension} válida. Por favor, suba un archivo de imagen real.");
        }
    }
}

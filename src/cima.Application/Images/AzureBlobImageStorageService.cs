using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
/// Almacenamiento de imagenes en Azure Blob Storage (Azul o Azurite en dev).
/// </summary>
public class AzureBlobImageStorageService : IImageStorageService, ITransientDependency
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ImageStorageOptions _options;
    private readonly ILogger<AzureBlobImageStorageService> _logger;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public AzureBlobImageStorageService(
        IOptions<ImageStorageOptions> options,
        ILogger<AzureBlobImageStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _blobServiceClient = new BlobServiceClient(_options.Azure.ConnectionString);
    }

    public async Task<UploadImageResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "listings")
    {
        ValidateInput(fileName, imageStream);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var supportsThumbnails = extension is ".jpg" or ".jpeg" or ".png" or ".webp";

        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var thumbnailName = $"{Path.GetFileNameWithoutExtension(uniqueName)}_thumb{extension}";

        await using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer);
        if (buffer.Length > MaxFileSize)
        {
            throw new UserFriendlyException($"El archivo excede el tamaño máximo de {MaxFileSize / (1024 * 1024)}MB");
        }

        buffer.Position = 0;
        ValidateImageMagicBytes(buffer, extension);

        var containerClient = await GetContainerClientAsync(folder);

        var originalUrl = await UploadBlobAsync(containerClient, uniqueName, buffer, extension);

        string thumbnailUrl;
        if (supportsThumbnails)
        {
            thumbnailUrl = await GenerateThumbnailAsync(containerClient, thumbnailName, buffer, extension)
                           ?? originalUrl;
        }
        else
        {
            thumbnailUrl = originalUrl;
        }

        return new UploadImageResult
        {
            Url = originalUrl,
            ThumbnailUrl = thumbnailUrl
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
            var (containerName, blobName) = ParseUrl(imageUrl);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? _options.Azure.ContainerName);
            await containerClient.DeleteBlobIfExistsAsync(blobName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar blob {ImageUrl}", imageUrl);
        }
    }

    public ImageValidationResult ValidateImage(string fileName, long fileSize)
    {
        if (fileSize > MaxFileSize)
        {
            return ImageValidationResult.Invalid(
                "ImageTooLarge",
                $"El archivo excede el tamaño máximo permitido ({MaxFileSize / (1024 * 1024)}MB)",
                ImageValidationSeverity.Error);
        }

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return ImageValidationResult.Invalid(
                "UnsupportedExtension",
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", AllowedExtensions)}",
                ImageValidationSeverity.Warning);
        }

        return ImageValidationResult.Valid();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string folder)
    {
        var containerName = string.IsNullOrWhiteSpace(folder) ? _options.Azure.ContainerName : folder.ToLowerInvariant();
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        return container;
    }

    private async Task<string> UploadBlobAsync(BlobContainerClient container, string blobName, Stream content, string extension)
    {
        content.Position = 0;
        var blob = container.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders
        {
            ContentType = GetContentType(extension)
        };

        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = headers
        };

        await blob.UploadAsync(content, uploadOptions);
        return BuildPublicUrl(container.Name, blobName, blob.Uri);
    }

    private async Task<string?> GenerateThumbnailAsync(BlobContainerClient container, string blobName, Stream source, string extension)
    {
        try
        {
            source.Position = 0;
            using var image = await Image.LoadAsync(source);

            const int targetWidth = 480;
            if (image.Width <= targetWidth)
            {
                return null;
            }

            var targetHeight = (int)Math.Max(1, Math.Round(image.Height * (targetWidth / (double)image.Width)));
            using var thumb = image.Clone(ctx => ctx.Resize(targetWidth, targetHeight));

            IImageEncoder encoder = extension switch
            {
                ".png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed },
                ".webp" => new WebpEncoder { Quality = 80 },
                _ => new JpegEncoder { Quality = 80 }
            };

            await using var ms = new MemoryStream();
            await thumb.SaveAsync(ms, encoder);
            return await UploadBlobAsync(container, blobName, ms, extension);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo generar thumbnail para {Blob}", blobName);
            return null;
        }
    }

    private void ValidateInput(string fileName, Stream imageStream)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new UserFriendlyException("El nombre del archivo es requerido");
        }

        if (imageStream == null || !imageStream.CanRead)
        {
            throw new UserFriendlyException("El stream de la imagen no es válido");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException(
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", AllowedExtensions)}");
        }
    }

    private void ValidateImageMagicBytes(Stream stream, string expectedExtension)
    {
        if (stream.Length < 12)
        {
            throw new UserFriendlyException("El archivo es demasiado pequeño para ser una imagen válida");
        }

        var buffer = new byte[12];
        stream.Position = 0;
        var bytesRead = stream.Read(buffer, 0, 12);
        stream.Position = 0;

        if (bytesRead < 4)
        {
            throw new UserFriendlyException("No se pudo leer el archivo correctamente");
        }

        var isValid = expectedExtension switch
        {
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38,
            ".webp" => bytesRead >= 12 &&
                       buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                       buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,
            _ => false
        };

        if (!isValid)
        {
            throw new UserFriendlyException(
                $"El archivo no es una imagen {expectedExtension} válida. Por favor, suba un archivo de imagen real.");
        }
    }

    private (string? container, string blobName) ParseUrl(string url)
    {
        try
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var path = uri.IsAbsoluteUri ? uri.AbsolutePath : url;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                var container = segments[0];
                var blob = string.Join('/', segments.Skip(1));
                return (container, blob);
            }
        }
        catch
        {
            // ignore and fallback
        }

        return (null, url.TrimStart('/'));
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }

    private string BuildPublicUrl(string container, string blobName, Uri blobUri)
    {
        if (!string.IsNullOrWhiteSpace(_options.Azure.PublicBaseUrl))
        {
            var baseUri = new Uri(_options.Azure.PublicBaseUrl.TrimEnd('/') + "/");
            return new Uri(baseUri, $"{container}/{blobName}").ToString();
        }

        return blobUri.ToString();
    }
}

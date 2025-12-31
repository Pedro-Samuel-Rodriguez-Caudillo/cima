using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    private const long MaxFileSize = ImageStorageHelper.DefaultMaxFileSize;
    private const int ThumbnailWidth = 480;

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
        ImageStorageHelper.ValidateFileName(fileName);
        ImageStorageHelper.ValidateReadable(imageStream);
        var extension = ImageStorageHelper.GetExtensionOrThrow(fileName);
        var supportsThumbnails = ImageStorageHelper.SupportsThumbnails(extension);

        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var thumbnailName = $"{Path.GetFileNameWithoutExtension(uniqueName)}_thumb{extension}";

        await using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer);
        ImageStorageHelper.ValidateFileSize(buffer.Length, MaxFileSize);
        ImageStorageHelper.ValidateImageMagicBytes(buffer, extension);

        var containerClient = await GetContainerClientAsync(folder);

        var originalUrl = await UploadBlobAsync(containerClient, uniqueName, buffer, extension);

        var thumbnailUrl = originalUrl;
        if (supportsThumbnails)
        {
            try
            {
                var thumbnailStream = await ImageStorageHelper.TryGenerateThumbnailAsync(
                    buffer,
                    extension,
                    ThumbnailWidth);

                if (thumbnailStream != null)
                {
                    thumbnailUrl = await UploadBlobAsync(containerClient, thumbnailName, thumbnailStream, extension);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo generar thumbnail para {Blob}", thumbnailName);
            }
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

        var ext = Path.GetExtension(fileName);
        if (!ImageStorageHelper.IsExtensionAllowed(ext))
        {
            return ImageValidationResult.Invalid(
                "UnsupportedExtension",
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", ImageStorageHelper.AllowedExtensions)}",
                ImageValidationSeverity.Warning);
        }

        return ImageValidationResult.Valid();
    }

    public async Task<Stream> GetImageStreamAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new UserFriendlyException("La URL de la imagen es requerida");
        }

        try
        {
            var (containerName, blobName) = ParseUrl(imageUrl);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? _options.Azure.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new UserFriendlyException($"La imagen no existe: {imageUrl}");
            }

            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen {ImageUrl}", imageUrl);
            throw new UserFriendlyException("Error al obtener la imagen");
        }
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
            var directUrl = new Uri(baseUri, $"{container}/{blobName}").ToString();
            
            // Si la URL es HTTP (Azurite local), usar el proxy HTTPS
            if (directUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                var proxyBaseUrl = _options.Azure.ProxyBaseUrl;
                if (!string.IsNullOrWhiteSpace(proxyBaseUrl))
                {
                    var encodedUrl = Uri.EscapeDataString(directUrl);
                    return $"{proxyBaseUrl.TrimEnd('/')}/api/image-proxy?url={encodedUrl}";
                }
            }
            
            return directUrl;
        }

        return blobUri.ToString();
    }
}

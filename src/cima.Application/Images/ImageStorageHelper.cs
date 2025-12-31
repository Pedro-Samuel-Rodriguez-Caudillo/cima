using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Volo.Abp;

namespace cima.Images;

internal static class ImageStorageHelper
{
    internal const long DefaultMaxFileSize = 5 * 1024 * 1024;
    internal static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    internal static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new UserFriendlyException("El nombre del archivo es requerido");
        }
    }

    internal static void ValidateReadable(Stream stream)
    {
        if (stream == null || !stream.CanRead)
        {
            throw new UserFriendlyException("El stream de la imagen no es valido");
        }
    }

    internal static bool IsExtensionAllowed(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return AllowedExtensions.Contains(extension.ToLowerInvariant());
    }

    internal static string GetExtensionOrThrow(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (!IsExtensionAllowed(extension))
        {
            throw new UserFriendlyException(
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", AllowedExtensions)}");
        }

        return extension.ToLowerInvariant();
    }

    internal static bool SupportsThumbnails(string extension)
        => extension is ".jpg" or ".jpeg" or ".png" or ".webp";

    internal static void ValidateFileSize(long size, long maxFileSize)
    {
        if (size > maxFileSize)
        {
            throw new UserFriendlyException(
                $"El archivo excede el tamano maximo de {maxFileSize / (1024 * 1024)}MB");
        }
    }

    internal static void ValidateImageMagicBytes(Stream stream, string expectedExtension)
    {
        if (stream.Length < 12)
        {
            throw new UserFriendlyException("El archivo es demasiado pequeno para ser una imagen valida");
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
                $"El archivo no es una imagen {expectedExtension} valida. Por favor, suba un archivo real.");
        }
    }

    internal static async Task<MemoryStream?> TryGenerateThumbnailAsync(
        Stream source,
        string extension,
        int targetWidth)
    {
        source.Position = 0;
        using var image = await Image.LoadAsync(source);

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

        var output = new MemoryStream();
        await thumb.SaveAsync(output, encoder);
        output.Position = 0;
        return output;
    }
}

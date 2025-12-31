using System;
using System.IO;
using System.Threading.Tasks;

namespace cima.Images;

/// <summary>
/// Servicio para almacenar y gestionar imágenes
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Sube una imagen y retorna URLs (original + thumbnail si aplica).
    /// </summary>
    /// <param name="imageStream">Stream de la imagen</param>
    /// <param name="fileName">Nombre original del archivo</param>
    /// <param name="folder">Carpeta destino (ej: "listings")</param>
    /// <returns>Resultado con URL y ThumbnailUrl</returns>
    Task<UploadImageResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "listings");
    
    /// <summary>
    /// Elimina una imagen por su URL
    /// </summary>
    /// <param name="imageUrl">URL relativa de la imagen</param>
    Task DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Obtiene el stream de una imagen almacenada.
    /// </summary>
    /// <param name="imageUrl">URL o nombre del blob</param>
    /// <returns>Stream del archivo</returns>
    Task<Stream> GetImageStreamAsync(string imageUrl);
    
    /// <summary>
    /// Valida que el archivo sea una imagen válida y describe la causa y gravedad en caso de error.
    /// </summary>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="fileSize">Tamaño en bytes</param>
    /// <returns>Resultado con estado y detalles de error</returns>
    ImageValidationResult ValidateImage(string fileName, long fileSize);
}

/// <summary>
/// Resultado de subida de imagen con URLs calculadas.
/// </summary>
public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public enum ImageValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public class ImageValidationResult
{
    public bool IsValid { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public ImageValidationSeverity Severity { get; init; }

    public static ImageValidationResult Valid()
    {
        return new ImageValidationResult
        {
            IsValid = true,
            Severity = ImageValidationSeverity.Info
        };
    }

    public static ImageValidationResult Invalid(string errorCode, string message, ImageValidationSeverity severity)
    {
        return new ImageValidationResult
        {
            IsValid = false,
            ErrorCode = errorCode,
            Message = message,
            Severity = severity
        };
    }
}

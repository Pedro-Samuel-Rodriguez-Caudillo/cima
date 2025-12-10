using System;
using System.IO;
using System.Threading.Tasks;

namespace cima.Images;

/// <summary>
/// Servicio para almacenar y gestionar im�genes
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
    /// Valida que el archivo sea una imagen v�lida
    /// </summary>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="fileSize">Tama�o en bytes</param>
    /// <returns>True si es v�lido</returns>
    bool ValidateImage(string fileName, long fileSize);
}

/// <summary>
/// Resultado de subida de imagen con URLs calculadas.
/// </summary>
public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

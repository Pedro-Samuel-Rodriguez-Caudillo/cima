using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "listings")
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

        // Generar nombre único
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        // Construir ruta completa (usando ContentRootPath)
        var uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "images", folder);
        
        // Crear directorio si no existe
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Guardar archivo
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"Error al guardar la imagen: {ex.Message}");
        }

        // Retornar URL relativa
        return $"/images/{folder}/{uniqueFileName}";
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
}

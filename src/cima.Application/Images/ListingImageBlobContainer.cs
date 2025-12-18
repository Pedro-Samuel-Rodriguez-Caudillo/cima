using Volo.Abp.BlobStoring;

namespace cima.Images;

/// <summary>
/// Container para almacenar imágenes de listings usando ABP BlobStoring.
/// Esto permite usar múltiples proveedores (Azure, AWS S3, MinIO, FileSystem).
/// </summary>
[BlobContainerName("listing-images")]
public class ListingImageBlobContainer
{
}

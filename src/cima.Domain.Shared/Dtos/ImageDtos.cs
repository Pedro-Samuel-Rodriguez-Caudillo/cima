using System;

namespace cima.Domain.Shared.Dtos;

/// <summary>
/// DTO para crear una imagen de listing
/// </summary>
public class CreateListingImageDto
{
    public required string Url { get; set; }  // ? required
    public int DisplayOrder { get; set; }
    public required string AltText { get; set; }  // ? required
    public long FileSize { get; set; }
    public required string ContentType { get; set; }  // ? required
}

/// <summary>
/// DTO para actualizar orden de imágenes
/// </summary>
public class UpdateImageOrderDto
{
    public Guid ImageId { get; set; }
    public int DisplayOrder { get; set; }
}

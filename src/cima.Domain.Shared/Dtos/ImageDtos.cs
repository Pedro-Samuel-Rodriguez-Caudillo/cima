using System;

namespace cima.Domain.Shared.Dtos;

/// <summary>
/// DTO para crear una imagen de listing
/// </summary>
public class CreateListingImageDto
{
    public string Url { get; set; }
    public int DisplayOrder { get; set; }
    public string AltText { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
}

/// <summary>
/// DTO para actualizar orden de imágenes
/// </summary>
public class UpdateImageOrderDto
{
    public Guid ImageId { get; set; }
    public int DisplayOrder { get; set; }
}

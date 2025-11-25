using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Represents an image associated with a listing, including metadata such as display order, alternative text, and
    /// file information.
    /// </summary>
    /// <remarks>This class is an immutable value object used to store and manage images for listings, enabling
    /// features such as image galleries, accessibility via alternative text, and ordering for display purposes.
    /// Instances are compared based on their atomic values.</remarks>
    public class ListingImage : ValueObject
    {
        public Guid ImageId { get; private set; }
        public string Url { get; private set; }
        public int DisplayOrder { get; private set; }
        public string AltText { get; private set; }
        public long FileSize { get; private set; }
        public string ContentType { get; private set; }

        // Constructor privado para EF Core
        private ListingImage()
        {
        }

        // Constructor público para crear instancias
        public ListingImage(
            Guid imageId,
            string url,
            int displayOrder,
            string altText,
            long fileSize,
            string contentType)
        {
            ImageId = imageId;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DisplayOrder = displayOrder;
            AltText = altText ?? string.Empty;
            FileSize = fileSize;
            ContentType = contentType ?? "image/jpeg";
        }

        // Método para actualizar DisplayOrder (permite reordenamiento)
        public ListingImage WithDisplayOrder(int newDisplayOrder)
        {
            return new ListingImage(ImageId, Url, newDisplayOrder, AltText, FileSize, ContentType);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ImageId;
            yield return Url;
            yield return DisplayOrder;
            yield return AltText;
            yield return FileSize;
            yield return ContentType;
        }
    }
}
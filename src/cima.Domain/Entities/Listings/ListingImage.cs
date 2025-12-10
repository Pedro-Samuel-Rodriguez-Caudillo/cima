using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Represents an image associated with a listing using a linked list structure.
    /// This enables efficient reordering without gaps or conflicts in display order.
    /// </summary>
    /// <remarks>
    /// Images are organized as a doubly-linked list where each image points to its
    /// previous and next sibling. This approach:
    /// - Eliminates display order conflicts (no two images with same order)
    /// - Enables O(1) insertion and deletion
    /// - Simplifies reordering operations
    /// - Maintains consistent ordering across operations
    /// </remarks>
    public class ListingImage : ValueObject
    {
        public Guid ImageId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string ThumbnailUrl { get; private set; } = string.Empty;
        
        /// <summary>
        /// ID of the previous image in the gallery (null if this is the first image)
        /// </summary>
        public Guid? PreviousImageId { get; private set; }
        
        /// <summary>
        /// ID of the next image in the gallery (null if this is the last image)
        /// </summary>
        public Guid? NextImageId { get; private set; }
        
        public string AltText { get; private set; } = string.Empty;
        public long FileSize { get; private set; }
        public string ContentType { get; private set; } = "image/jpeg";

        // Constructor privado para EF Core
        private ListingImage()
        {
        }

        // Constructor público para crear instancias
        public ListingImage(
            Guid imageId,
            string url,
            string thumbnailUrl,
            string altText,
            long fileSize,
            string contentType,
            Guid? previousImageId = null,
            Guid? nextImageId = null)
        {
            ImageId = imageId;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            ThumbnailUrl = thumbnailUrl ?? url ?? string.Empty;
            PreviousImageId = previousImageId;
            NextImageId = nextImageId;
            AltText = altText ?? string.Empty;
            FileSize = fileSize;
            ContentType = contentType ?? "image/jpeg";
        }

        /// <summary>
        /// Creates a new image instance with updated linked list pointers
        /// </summary>
        public ListingImage WithLinks(Guid? previousImageId, Guid? nextImageId)
        {
            return new ListingImage(
                ImageId, 
                Url, 
                ThumbnailUrl,
                AltText, 
                FileSize, 
                ContentType,
                previousImageId,
                nextImageId
            );
        }

        /// <summary>
        /// Creates a new image instance with updated previous pointer
        /// </summary>
        public ListingImage WithPreviousImage(Guid? previousImageId)
        {
            return WithLinks(previousImageId, NextImageId);
        }

        /// <summary>
        /// Creates a new image instance with updated next pointer
        /// </summary>
        public ListingImage WithNextImage(Guid? nextImageId)
        {
            return WithLinks(PreviousImageId, nextImageId);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ImageId;
            yield return Url;
            yield return ThumbnailUrl;
            yield return PreviousImageId ?? Guid.Empty;
            yield return NextImageId ?? Guid.Empty;
            yield return AltText;
            yield return FileSize;
            yield return ContentType;
        }
    }
}
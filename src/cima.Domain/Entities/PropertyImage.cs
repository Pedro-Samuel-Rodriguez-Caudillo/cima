using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Represents an image associated with a property, including metadata such as display order, alternative text, and
    /// file information.
    /// </summary>
    /// <remarks>This class is typically used to store and manage images for property listings, enabling
    /// features such as image galleries, accessibility via alternative text, and ordering for display purposes.
    /// Instances of this class are considered value objects and are compared based on their atomic values.</remarks>
    /// 
    /// O en palabras más sencillas son metadatos.
    public class PropertyImage : ValueObject
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; } // Texto alternativo para accesibilidad (No quiero penalizaciones de CEO)
        public long FileSize { get; set; }
        public string ContentType { get; set; } // Tipo de contenido (e.g., "image/jpeg", "image/png")

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ImageId;
            yield return Url;
            yield return DisplayOrder;
        }
    }
}
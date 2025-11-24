using System;
using System.Collections.Generic;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    public class ListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
        public PropertyType Type { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDto Architect { get; set; }
        public List<ListingImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        
        // Propiedades de conveniencia
        public string Address => Location; // Alias para compatibilidad
        public DateTime CreationTime => CreatedAt; // Alias para compatibilidad
    }

    public class CreateUpdateListingDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public PropertyType Type { get; set; } = PropertyType.House;
        public TransactionType TransactionType { get; set; } = TransactionType.Sale;
        public Guid ArchitectId { get; set; }
    }

    public class ListingImageDto
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
    }
}

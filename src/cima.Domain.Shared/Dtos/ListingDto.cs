using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    public class ListingDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }  // ✅ required
        public required string Description { get; set; }  // ✅ required
        public string? Location { get; set; }  // ✅ nullable
        public decimal Price { get; set; }
        public decimal LandArea { get; set; }
        public decimal ConstructionArea { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
        
        // Normalización de tipos de propiedad
        public PropertyCategory Category { get; set; }
        public PropertyType Type { get; set; }
        
        public TransactionType TransactionType { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDto? Architect { get; set; }  // ✅ nullable - puede no estar cargado
        public List<ListingImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        
        // Propiedades de conveniencia (backward compatibility)
        public string? Address => Location;
        public DateTime CreationTime => CreatedAt;
        public decimal Area => LandArea; // Para compatibilidad con código existente
    }

    public class CreateUpdateListingDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 200 caracteres")]
        public required string Title { get; set; }  // ✅ required

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(5000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 5000 caracteres")]
        public required string Description { get; set; }  // ✅ required

        [Required(ErrorMessage = "La ubicación es requerida")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "La ubicación debe tener entre 5 y 500 caracteres")]
        public string? Location { get; set; }  // ✅ nullable

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El área del terreno es requerida")]
        [Range(1, 100000, ErrorMessage = "El área del terreno debe estar entre 1 y 100000 m²")]
        public decimal LandArea { get; set; }

        [Required(ErrorMessage = "El área de construcción es requerida")]
        [Range(1, 100000, ErrorMessage = "El área de construcción debe estar entre 1 y 100000 m²")]
        public decimal ConstructionArea { get; set; }

        [Required(ErrorMessage = "El número de recámaras es requerido")]
        [Range(0, 50, ErrorMessage = "El número de recámaras debe estar entre 0 y 50")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "El número de baños es requerido")]
        [Range(0, 50, ErrorMessage = "El número de baños debe estar entre 0 y 50")]
        public int Bathrooms { get; set; }
        
        [Required(ErrorMessage = "La categoría es requerida")]
        public PropertyCategory Category { get; set; } = PropertyCategory.Residential;

        [Required(ErrorMessage = "El tipo de propiedad es requerido")]
        public PropertyType Type { get; set; } = PropertyType.House;
        
        [Required(ErrorMessage = "El tipo de transacción es requerido")]
        public TransactionType TransactionType { get; set; } = TransactionType.Sale;

        [Required(ErrorMessage = "El arquitecto es requerido")]
        public Guid ArchitectId { get; set; }
        
        // Backward compatibility
        [Obsolete("Use LandArea instead")]
        public decimal? Area { get; set; }
    }

    public class ListingImageDto
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public Guid? PreviousImageId { get; set; }  // ✨ Lista enlazada
        public Guid? NextImageId { get; set; }      // ✨ Lista enlazada
        public string AltText { get; set; } = string.Empty;
    }
}

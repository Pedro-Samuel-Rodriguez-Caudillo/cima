using System;
using System.Collections.Generic;

namespace cima.Domain.Shared.Dtos
{
    public class ArchitectDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }  // Nombre del usuario de Identity
        
        // Estadísticas
        public int TotalListingsPublished { get; set; }
        public int ActiveListings { get; set; }
        
        // Metadata
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateArchitectDto
    {
        // Sin campos - se crea vinculado al usuario actual
    }
    
    public class UpdateArchitectDto
    {
        // Solo admin puede modificar IsActive
        public bool? IsActive { get; set; }
    }

    public class ArchitectDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int TotalListingsPublished { get; set; }
        public int ActiveListings { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public List<ListingListDto> Listings { get; set; } = new();
    }

    public class ArchitectListDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public int TotalListingsPublished { get; set; }
        public int ActiveListings { get; set; }
        public bool IsActive { get; set; }
    }
}

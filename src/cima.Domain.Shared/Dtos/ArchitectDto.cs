using System;
using System.Collections.Generic;

namespace cima.Domain.Shared.Dtos
{
    public class ArchitectDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;  // ? Agregado
        public string? Bio { get; set; }  // ? Nullable
        public string? UserName { get; set; }  // Nombre del usuario de Identity
    }

    public class CreateArchitectDto
    {
        public string Name { get; set; } = string.Empty;  // ? Agregado - requerido
        public string? Bio { get; set; }  // ? Nullable - opcional
    }
    
    public class UpdateArchitectDto
    {
        public string? Name { get; set; }  // ? Agregado - opcional en update
        public string? Bio { get; set; }  // ? Nullable
    }

    public class ArchitectDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;  // ? Agregado
        public string? Bio { get; set; }  // ? Nullable
        public string? UserName { get; set; }
        public List<ListingListDto> Listings { get; set; } = new();
    }

    public class ArchitectListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;  // ? Agregado
        public string? UserName { get; set; }
        public string? Bio { get; set; }  // ? Nullable
        public int ListingsCount { get; set; }
    }
}

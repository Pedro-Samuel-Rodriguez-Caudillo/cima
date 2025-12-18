using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace cima.Domain.Entities;

/// <summary>
/// Categoría de propiedad administrable por el usuario.
/// Reemplaza el enum PropertyCategory para permitir CRUD dinámico.
/// </summary>
public class PropertyCategoryEntity : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Nombre de la categoría (ej: "Residencial", "Comercial")
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descripción opcional
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int SortOrder { get; private set; }
    
    /// <summary>
    /// Si la categoría está activa (visible en formularios)
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Icono opcional (FontAwesome class, ej: "fa-home")
    /// </summary>
    public string? Icon { get; private set; }

    // Constructor para EF Core
    private PropertyCategoryEntity() { }
    
    public PropertyCategoryEntity(
        Guid id,
        string name,
        string? description,
        int sortOrder,
        bool isActive = true,
        string? icon = null)
        : base(id)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        Icon = icon;
    }
    
    public void Update(string name, string? description, int sortOrder, bool isActive, string? icon)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        Icon = icon;
    }
    
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace cima.Domain.Entities;

/// <summary>
/// Tipo de propiedad administrable por el usuario.
/// Reemplaza el enum PropertyType para permitir CRUD dinámico.
/// </summary>
public class PropertyTypeEntity : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Nombre del tipo (ej: "Casa", "Departamento")
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Categoría padre a la que pertenece este tipo
    /// </summary>
    public Guid CategoryId { get; private set; }
    
    /// <summary>
    /// Descripción opcional
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int SortOrder { get; private set; }
    
    /// <summary>
    /// Si el tipo está activo
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    // Navegación
    public virtual PropertyCategoryEntity? Category { get; private set; }

    // Constructor para EF Core
    private PropertyTypeEntity() { }
    
    public PropertyTypeEntity(
        Guid id,
        string name,
        Guid categoryId,
        string? description,
        int sortOrder,
        bool isActive = true)
        : base(id)
    {
        Name = name;
        CategoryId = categoryId;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
    
    public void Update(string name, Guid categoryId, string? description, int sortOrder, bool isActive)
    {
        Name = name;
        CategoryId = categoryId;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}

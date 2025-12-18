using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace cima.Domain.Entities;

/// <summary>
/// Tipo de transacción administrable por el usuario.
/// Reemplaza el enum TransactionType para permitir CRUD dinámico.
/// </summary>
public class TransactionTypeEntity : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Nombre del tipo (ej: "Venta", "Renta")
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Código interno (ej: "SALE", "RENT") para compatibilidad
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    
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

    // Constructor para EF Core
    private TransactionTypeEntity() { }
    
    public TransactionTypeEntity(
        Guid id,
        string name,
        string code,
        string? description,
        int sortOrder,
        bool isActive = true)
        : base(id)
    {
        Name = name;
        Code = code;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
    
    public void Update(string name, string code, string? description, int sortOrder, bool isActive)
    {
        Name = name;
        Code = code;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
    
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

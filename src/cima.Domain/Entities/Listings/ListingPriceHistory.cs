using System;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities.Listings;

/// <summary>
/// Historial de cambios de precio de un Listing.
/// Incluye metadatos de auditoría para detección de fraudes.
/// </summary>
public class ListingPriceHistory : Entity<Guid>
{
    public Guid ListingId { get; private set; }
    
    /// <summary>
    /// Precio anterior antes del cambio
    /// </summary>
    public decimal OldPrice { get; private set; }
    
    /// <summary>
    /// Precio nuevo después del cambio
    /// </summary>
    public decimal NewPrice { get; private set; }
    
    /// <summary>
    /// Fecha y hora UTC del cambio
    /// </summary>
    public DateTime ChangedAt { get; private set; }
    
    // ========================================
    // METADATOS ANTI-FRAUDE
    // ========================================
    
    /// <summary>
    /// ID del usuario que realizó el cambio
    /// </summary>
    public Guid? ChangedByUserId { get; private set; }
    
    /// <summary>
    /// Nombre del usuario para referencia histórica
    /// (el username puede cambiar, pero este queda fijo)
    /// </summary>
    public string? ChangedByUserName { get; private set; }
    
    /// <summary>
    /// Dirección IP del cliente que realizó el cambio.
    /// Crucial para rastrear accesos no autorizados.
    /// </summary>
    public string? ClientIpAddress { get; private set; }
    
    /// <summary>
    /// User-Agent del navegador/cliente.
    /// Ayuda a identificar el dispositivo usado.
    /// </summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// ID de correlación de la request HTTP.
    /// Permite encontrar logs relacionados.
    /// </summary>
    public string? CorrelationId { get; private set; }
    
    /// <summary>
    /// Razón opcional del cambio de precio
    /// </summary>
    public string? ChangeReason { get; private set; }
    
    /// <summary>
    /// ID de la sesión del usuario (si está disponible)
    /// </summary>
    public string? SessionId { get; private set; }
    
    /// <summary>
    /// Método de autenticación usado (Cookie, Bearer, etc.)
    /// </summary>
    public string? AuthenticationMethod { get; private set; }
    
    // ========================================
    // ANTI-TAMPERING (Integridad de datos)
    // ========================================
    
    /// <summary>
    /// Hash SHA256 del registro anterior (blockchain-lite).
    /// Si es null, es el primer registro del listing.
    /// </summary>
    public string? PreviousRecordHash { get; private set; }
    
    /// <summary>
    /// Hash SHA256 de este registro.
    /// Se calcula con: ListingId + OldPrice + NewPrice + ChangedAt + ChangedByUserId + PreviousRecordHash
    /// </summary>
    public string IntegrityHash { get; private set; } = string.Empty;

    // Constructor para EF Core
    private ListingPriceHistory() { }
    
    public ListingPriceHistory(
        Guid id,
        Guid listingId,
        decimal oldPrice,
        decimal newPrice,
        Guid? changedByUserId,
        string? changedByUserName,
        string? clientIpAddress,
        string? userAgent,
        string? correlationId,
        string? changeReason,
        string? sessionId,
        string? authenticationMethod,
        string? previousRecordHash)
        : base(id)
    {
        ListingId = listingId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        ChangedAt = DateTime.UtcNow;
        ChangedByUserId = changedByUserId;
        ChangedByUserName = changedByUserName;
        ClientIpAddress = clientIpAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        ChangeReason = changeReason;
        SessionId = sessionId;
        AuthenticationMethod = authenticationMethod;
        PreviousRecordHash = previousRecordHash;
        
        // Calcular hash de integridad
        IntegrityHash = ComputeIntegrityHash();
    }
    
    /// <summary>
    /// Calcula el hash SHA256 para verificación de integridad.
    /// Si el hash almacenado no coincide con el recalculado, el registro fue manipulado.
    /// </summary>
    public string ComputeIntegrityHash()
    {
        var data = $"{ListingId}|{OldPrice}|{NewPrice}|{ChangedAt:O}|{ChangedByUserId}|{PreviousRecordHash}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes);
    }
    
    /// <summary>
    /// Verifica si el registro mantiene su integridad.
    /// </summary>
    public bool VerifyIntegrity()
    {
        return IntegrityHash == ComputeIntegrityHash();
    }
}

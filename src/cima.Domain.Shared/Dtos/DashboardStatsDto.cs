using System;
using System.Collections.Generic;

namespace cima.Domain.Shared.Dtos;

/// <summary>
/// Estadísticas generales del dashboard administrativo
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total de propiedades en sistema
    /// </summary>
    public int TotalListings { get; set; }

    /// <summary>
    /// Propiedades publicadas (visibles públicamente)
    /// </summary>
    public int PublishedListings { get; set; }

    /// <summary>
    /// Propiedades en borrador
    /// </summary>
    public int DraftListings { get; set; }

    /// <summary>
    /// Propiedades archivadas
    /// </summary>
    public int ArchivedListings { get; set; }

    /// <summary>
    /// Total de arquitectos registrados
    /// </summary>
    public int TotalArchitects { get; set; }

    /// <summary>
    /// Arquitectos con al menos una propiedad
    /// </summary>
    public int ActiveArchitects { get; set; }

    /// <summary>
    /// Total de solicitudes de contacto
    /// </summary>
    public int TotalContactRequests { get; set; }

    /// <summary>
    /// Solicitudes pendientes de respuesta
    /// </summary>
    public int PendingContactRequests { get; set; }

    /// <summary>
    /// Solicitudes cerradas
    /// </summary>
    public int ClosedContactRequests { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Estadísticas detalladas de propiedades
/// </summary>
public class ListingStatsDto
{
    /// <summary>
    /// Conteo por tipo de propiedad
    /// </summary>
    public Dictionary<string, int> ByType { get; set; } = new();

    /// <summary>
    /// Conteo por tipo de transacción
    /// </summary>
    public Dictionary<string, int> ByTransaction { get; set; } = new();

    /// <summary>
    /// Conteo por estado
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    /// <summary>
    /// Propiedades creadas en últimos 30 días
    /// </summary>
    public int CreatedLast30Days { get; set; }

    /// <summary>
    /// Precio promedio de propiedades publicadas
    /// </summary>
    public decimal AveragePrice { get; set; }
}

/// <summary>
/// Estadísticas de solicitudes de contacto
/// </summary>
public class ContactRequestStatsDto
{
    /// <summary>
    /// Solicitudes por día (últimos 30 días)
    /// </summary>
    public Dictionary<string, int> RequestsPerDay { get; set; } = new();

    /// <summary>
    /// Conteo por estado
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    /// <summary>
    /// Tiempo promedio de respuesta (en horas)
    /// </summary>
    public double AverageResponseTimeHours { get; set; }

    /// <summary>
    /// Solicitudes sin responder
    /// </summary>
    public int UnrepliedCount { get; set; }
}

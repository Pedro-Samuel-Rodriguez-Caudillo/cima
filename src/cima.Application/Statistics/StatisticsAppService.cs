using System;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace cima.Statistics;

/// <summary>
/// Implementación del servicio de estadísticas
/// Requiere permisos administrativos para todos los métodos
/// </summary>
[Authorize(cimaPermissions.Listings.Default)] // Requiere al menos permiso de lectura
public class StatisticsAppService : ApplicationService, IStatisticsAppService
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;

    public StatisticsAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IRepository<ContactRequest, Guid> contactRequestRepository)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _contactRequestRepository = contactRequestRepository;
    }

    /// <summary>
    /// Obtiene estadísticas generales del dashboard
    /// </summary>
    public async Task<DashboardStatsDto> GetDashboardAsync()
    {
        var listingsQuery = await _listingRepository.GetQueryableAsync();
        var architectsQuery = await _architectRepository.GetQueryableAsync();
        var contactRequestsQuery = await _contactRequestRepository.GetQueryableAsync();

        // Estadísticas de Listings
        var totalListings = await AsyncExecuter.CountAsync(listingsQuery);
        var publishedListings = await AsyncExecuter.CountAsync(
            listingsQuery.Where(l => l.Status == ListingStatus.Published)
        );
        var draftListings = await AsyncExecuter.CountAsync(
            listingsQuery.Where(l => l.Status == ListingStatus.Draft)
        );
        var archivedListings = await AsyncExecuter.CountAsync(
            listingsQuery.Where(l => l.Status == ListingStatus.Archived)
        );

        // Estadísticas de Arquitectos
        var totalArchitects = await AsyncExecuter.CountAsync(architectsQuery);
        var activeArchitects = await AsyncExecuter.CountAsync(
            architectsQuery.Where(a => 
                listingsQuery.Any(l => l.ArchitectId == a.Id)
            )
        );

        // Estadísticas de ContactRequests - CORREGIDO: usar "New" en lugar de "Pending"
        var totalContactRequests = await AsyncExecuter.CountAsync(contactRequestsQuery);
        var pendingContactRequests = await AsyncExecuter.CountAsync(
            contactRequestsQuery.Where(cr => cr.Status == ContactRequestStatus.New)
        );
        var closedContactRequests = await AsyncExecuter.CountAsync(
            contactRequestsQuery.Where(cr => cr.Status == ContactRequestStatus.Closed)
        );

        return new DashboardStatsDto
        {
            TotalListings = totalListings,
            PublishedListings = publishedListings,
            DraftListings = draftListings,
            ArchivedListings = archivedListings,
            TotalArchitects = totalArchitects,
            ActiveArchitects = activeArchitects,
            TotalContactRequests = totalContactRequests,
            PendingContactRequests = pendingContactRequests,
            ClosedContactRequests = closedContactRequests,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Obtiene estadísticas detalladas de propiedades
    /// </summary>
    public async Task<ListingStatsDto> GetListingStatsAsync()
    {
        var query = await _listingRepository.GetQueryableAsync();
        var listings = await AsyncExecuter.ToListAsync(query);

        var stats = new ListingStatsDto();

        // Agrupar por tipo
        stats.ByType = listings
            .GroupBy(l => l.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Agrupar por transacción
        stats.ByTransaction = listings
            .GroupBy(l => l.TransactionType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Agrupar por estado
        stats.ByStatus = listings
            .GroupBy(l => l.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Propiedades creadas en últimos 30 días
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        stats.CreatedLast30Days = listings.Count(l => l.CreatedAt >= thirtyDaysAgo);

        // Precio promedio (solo publicadas)
        var publishedListings = listings.Where(l => l.Status == ListingStatus.Published).ToList();
        stats.AveragePrice = publishedListings.Any() 
            ? publishedListings.Average(l => l.Price) 
            : 0;

        return stats;
    }

    /// <summary>
    /// Obtiene estadísticas de solicitudes de contacto
    /// </summary>
    public async Task<ContactRequestStatsDto> GetContactRequestStatsAsync()
    {
        var query = await _contactRequestRepository.GetQueryableAsync();
        var contactRequests = await AsyncExecuter.ToListAsync(query);

        var stats = new ContactRequestStatsDto();

        // Solicitudes por día (últimos 30 días)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        stats.RequestsPerDay = contactRequests
            .Where(cr => cr.CreatedAt >= thirtyDaysAgo)
            .GroupBy(cr => cr.CreatedAt.Date.ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        // Agrupar por estado
        stats.ByStatus = contactRequests
            .GroupBy(cr => cr.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // ACTUALIZADO: Calcular tiempo promedio de respuesta usando RepliedAt
        var repliedRequests = contactRequests
            .Where(cr => cr.RepliedAt.HasValue)
            .ToList();

        if (repliedRequests.Any())
        {
            var totalHours = repliedRequests
                .Sum(cr => (cr.RepliedAt!.Value - cr.CreatedAt).TotalHours);
            stats.AverageResponseTimeHours = totalHours / repliedRequests.Count;
        }
        else
        {
            stats.AverageResponseTimeHours = 0;
        }

        // Solicitudes sin responder (New)
        stats.UnrepliedCount = contactRequests.Count(cr => cr.Status == ContactRequestStatus.New);

        return stats;
    }
}
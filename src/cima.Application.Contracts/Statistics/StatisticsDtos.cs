using System;
using System.Collections.Generic;

namespace cima.Statistics;

public class DashboardStatsRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DashboardStatsDto
{
    public int TotalListings { get; set; }
    public int PublishedListings { get; set; }
    public int DraftListings { get; set; }
    public int ArchivedListings { get; set; }
    public int PortfolioListings { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public Dictionary<string, decimal> SalesByMonth { get; set; } = new();
    public int TotalArchitects { get; set; }
    public int ActiveArchitects { get; set; }
    public int TotalContactRequests { get; set; }
    public int PendingContactRequests { get; set; }
    public int ClosedContactRequests { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ListingStatsDto
{
    public Dictionary<string, int> ByType { get; set; } = new();
    public Dictionary<string, int> ByTransaction { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public int CreatedLast30Days { get; set; }
    public decimal AveragePrice { get; set; }
}

public class ContactRequestStatsDto
{
    public Dictionary<string, int> RequestsPerDay { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public double AverageResponseTimeHours { get; set; }
    public int UnrepliedCount { get; set; }
}

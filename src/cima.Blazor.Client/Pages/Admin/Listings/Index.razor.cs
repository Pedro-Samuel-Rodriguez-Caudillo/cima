using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using cima.Blazor.Client;
using cima.Blazor.Client.Services;
using cima.Domain.Shared;
using cima.Listings;
using cima.Listings.Inputs;
using cima.Listings.Outputs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace cima.Blazor.Client.Pages.Admin.Listings;

public partial class Index : cimaComponentBase
{
    private readonly Timer _searchTimer = new(500) { AutoReset = false };

    private List<ListingSummaryDto> Listings { get; set; } = new();
    private HashSet<Guid> SelectedIds { get; set; } = new();
    private int TotalCount { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;
    private bool IsLoading { get; set; } = true;
    private string SearchTerm { get; set; } = string.Empty;
    private string? StatusFilter { get; set; }
    private string? CategoryFilter { get; set; }
    private string? TransactionFilter { get; set; }
    private HashSet<Guid> FeaturedListingIds { get; set; } = new();
    private int FeaturedListingCount { get; set; }
    private const int MaxFeaturedListings = 12;

    private bool IsAllSelected => Listings.Any() && Listings.All(l => SelectedIds.Contains(l.Id));
    private static readonly CultureInfo MexicoCulture = new("es-MX");

    [Inject] private IListingAppService ListingService { get; set; } = default!;
    [Inject] private IFeaturedListingAppService FeaturedListingService { get; set; } = default!;
    [Inject] protected EnumLocalizationService EnumLocalizer { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _searchTimer.Elapsed += OnSearchElapsed;
        await LoadListings();
    }

    private async void OnSearchElapsed(object? sender, ElapsedEventArgs e)
    {
        CurrentPage = 1;
        await InvokeAsync(LoadListings);
    }

    private void HandleSearchKeyUp()
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private async Task LoadListings()
    {
        try
        {
            IsLoading = true;
            var input = BuildFilterInput();
            var resultTask = ListingService.GetListAsync(input);
            var featuredTask = LoadFeaturedListingsAsync();
            await Task.WhenAll(resultTask, featuredTask);
            var result = await resultTask;
            Listings = result.Items.ToList();
            TotalCount = (int)result.TotalCount;

            SelectedIds.RemoveWhere(id => !Listings.Any(l => l.Id == id));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadFeaturedListingsAsync()
    {
        try
        {
            var featuredListings = await FeaturedListingService.GetAllAsync();
            FeaturedListingIds = featuredListings
                .Select(featured => featured.ListingId)
                .ToHashSet();
            FeaturedListingCount = FeaturedListingIds.Count;
        }
        catch
        {
            FeaturedListingIds = new HashSet<Guid>();
            FeaturedListingCount = 0;
        }
    }

    private bool IsFeatured(Guid listingId) => FeaturedListingIds.Contains(listingId);

    private bool CanToggleFeatured(ListingSummaryDto listing)
    {
        if (IsFeatured(listing.Id))
        {
            return true;
        }

        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            return false;
        }

        return FeaturedListingCount < MaxFeaturedListings;
    }

    private string GetFeaturedActionLabel(ListingSummaryDto listing)
    {
        if (IsFeatured(listing.Id))
        {
            return L["Admin:Listings:Featured:Remove"];
        }

        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            return L["Admin:Listings:Featured:RequiresPublished"];
        }

        if (FeaturedListingCount >= MaxFeaturedListings)
        {
            return string.Format(L["Admin:Listings:Featured:LimitReached"], MaxFeaturedListings);
        }

        return L["Admin:Listings:Featured:Add"];
    }

    private async Task ToggleFeaturedAsync(ListingSummaryDto listing)
    {
        if (IsFeatured(listing.Id))
        {
            await ExecuteWithReload(async () =>
                {
                    await FeaturedListingService.RemoveByListingIdAsync(listing.Id);
                    FeaturedListingIds.Remove(listing.Id);
                    FeaturedListingCount = Math.Max(0, FeaturedListingCount - 1);
                },
                L["Admin:Listings:Featured:Removed"]);
            return;
        }

        if (!CanToggleFeatured(listing))
        {
            Snackbar.Add(GetFeaturedActionLabel(listing), Severity.Warning);
            return;
        }

        await ExecuteWithReload(async () =>
            {
                await FeaturedListingService.AddAsync(new CreateFeaturedListingDto { ListingId = listing.Id });
                FeaturedListingIds.Add(listing.Id);
                FeaturedListingCount++;
            },
            L["Admin:Listings:Featured:Added"]);
    }

    private GetListingsInput BuildFilterInput() =>
        new()
        {
            SkipCount = (CurrentPage - 1) * PageSize,
            MaxResultCount = PageSize,
            Sorting = "CreatedAt DESC",
            SearchTerm = SearchTerm,
            Status = ParseNullable(StatusFilter),
            PropertyCategory = ParseNullable(CategoryFilter),
            TransactionType = ParseNullable(TransactionFilter)
        };

    private static int? ParseNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : int.Parse(value);

    private void ToggleSelection(Guid id)
    {
        if (SelectedIds.Contains(id))
        {
            SelectedIds.Remove(id);
            return;
        }

        SelectedIds.Add(id);
    }

    private void ToggleSelectAll()
    {
        if (!Listings.Any())
        {
            return;
        }

        if (IsAllSelected)
        {
            SelectedIds.Clear();
            return;
        }

        foreach (var listing in Listings)
        {
            SelectedIds.Add(listing.Id);
        }
    }

    private void ClearSelection() => SelectedIds.Clear();

    private async Task BulkPublish() =>
        await ConfirmAndExecuteBulk(
            "Publicar propiedades",
            $"�Publicar {SelectedIds.Count} propiedades seleccionadas?",
            id => ListingService.PublishAsync(new PublishListingDto { ListingId = id }),
            count => $"{count} propiedades publicadas");

    private async Task BulkArchive() =>
        await ConfirmAndExecuteBulk(
            "Archivar propiedades",
            $"�Archivar {SelectedIds.Count} propiedades seleccionadas?",
            id => ListingService.ArchiveAsync(id),
            count => $"{count} propiedades archivadas");

    private async Task BulkMoveToPortfolio() =>
        await ConfirmAndExecuteBulk(
            "Mover a Portafolio",
            $"�Mover {SelectedIds.Count} propiedades al portafolio?",
            id => ListingService.MoveToPortfolioAsync(id),
            count => $"{count} propiedades movidas al portafolio");

    private async Task BulkDelete() =>
        await ConfirmAndExecuteBulk(
            "Eliminar propiedades",
            $"�Eliminar {SelectedIds.Count} propiedades? Esta acción no se puede deshacer.",
            id => ListingService.DeleteAsync(id),
            count => $"{count} propiedades eliminadas");

    private async Task ConfirmAndExecuteBulk(
        string title,
        string message,
        Func<Guid, Task> action,
        Func<int, string> successMessage)
    {
        if (!SelectedIds.Any())
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBox(title, message, yesText: "Confirmar", cancelText: "Cancelar");
        if (confirmed != true)
        {
            return;
        }

        var success = 0;
        foreach (var id in SelectedIds.ToList())
        {
            try
            {
                await action(id);
                success++;
            }
            catch
            {
                // Ignorar errores individuales para no interrumpir el lote
            }
        }

        Snackbar.Add(successMessage(success), Severity.Success);
        ClearSelection();
        await LoadListings();
    }

    private async Task PublishListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.PublishAsync(new PublishListingDto { ListingId = id }), "Propiedad publicada");

    private async Task ArchiveListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.ArchiveAsync(id), "Propiedad archivada");

    private async Task UnpublishListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.UnpublishAsync(id), "Propiedad enviada a borrador");

    private async Task UnarchiveListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.UnarchiveAsync(id), "Propiedad desarchivada");

    private async Task MoveListingToPortfolio(Guid id) =>
        await ExecuteWithReload(() => ListingService.MoveToPortfolioAsync(id), "Propiedad movida al portafolio");

    private async Task DuplicateListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.DuplicateAsync(id), "Propiedad duplicada exitosamente");

    private async Task DeleteListing(Guid id)
    {
        var result = await DialogService.ShowMessageBox(
            "Eliminar Propiedad",
            "�Estás seguro de eliminar esta propiedad? Esta acción no se puede deshacer.",
            yesText: "Eliminar", cancelText: "Cancelar");

        if (result != true)
        {
            return;
        }

        await ExecuteWithReload(() => ListingService.DeleteAsync(id), "Propiedad eliminada");
    }

    private async Task ExecuteWithReload(Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            Snackbar.Add(successMessage, Severity.Success);
            await LoadListings();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task GoToPage(int page)
    {
        CurrentPage = page;
        await LoadListings();
    }

    private Color GetStatusColor(ListingStatus status) => status switch
    {
        ListingStatus.Published => Color.Success,
        ListingStatus.Draft => Color.Warning,
        ListingStatus.Archived => Color.Default,
        ListingStatus.Portfolio => Color.Info,
        _ => Color.Default
    };

    private string FormatPrice(ListingSummaryDto listing)
    {
        if (listing.IsPriceOnRequest || !listing.Price.HasValue)
        {
            return L["Common:PriceOnRequest"];
        }

        return listing.Price.Value.ToString("C0", MexicoCulture);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _searchTimer.Elapsed -= OnSearchElapsed;
        _searchTimer.Dispose();
    }
}





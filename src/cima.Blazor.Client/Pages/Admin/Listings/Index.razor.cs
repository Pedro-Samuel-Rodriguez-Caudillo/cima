using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using cima.Blazor.Client;
using cima.Blazor.Client.Components.Common;
using cima.Blazor.Client.Services;
using cima.Categories;
using cima.Domain.Shared;
using cima.Listings;
using cima.Portfolio;
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
    private List<PropertyCategoryDto> Categories { get; set; } = new();
    private int TotalCount { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;
    private bool IsLoading { get; set; } = true;
    private string SearchTerm { get; set; } = string.Empty;
    private string? StatusFilter { get; set; }
    private string? CategoryFilter { get; set; }
    private string? TransactionFilter { get; set; }
    private string? FeaturedFilter { get; set; }
    private HashSet<Guid> FeaturedListingIds { get; set; } = new();
    private int FeaturedListingCount { get; set; }
    private const int MaxFeaturedListings = 12;

    private bool IsAllSelected => Listings.Any() && Listings.All(l => SelectedIds.Contains(l.Id));
    private static readonly CultureInfo MexicoCulture = new("es-MX");

    [Inject] private IListingAppService ListingService { get; set; } = default!;
    [Inject] private IPortfolioAppService PortfolioService { get; set; } = default!;
    [Inject] private IFeaturedListingAppService FeaturedListingService { get; set; } = default!;
    [Inject] private ICategoryAppService CategoryService { get; set; } = default!;
    [Inject] protected EnumLocalizationService EnumLocalizer { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _searchTimer.Elapsed += OnSearchElapsed;
        await LoadCategoriesAsync();
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

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await CategoryService.GetCategoriesAsync();
            Categories = categories
                .Where(category => category.IsActive)
                .OrderBy(category => category.SortOrder)
                .ToList();
        }
        catch
        {
            Categories = new List<PropertyCategoryDto>();
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
            CategoryId = ParseNullableGuid(CategoryFilter),
            TransactionType = ParseNullable(TransactionFilter),
            FeaturedOnly = string.Equals(FeaturedFilter, "featured", StringComparison.OrdinalIgnoreCase)
        };

    private static int? ParseNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : int.Parse(value);

    private static Guid? ParseNullableGuid(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Guid.Parse(value);

    private async Task HandleFilterChanged()
    {
        CurrentPage = 1;
        await LoadListings();
    }

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

    private const string BulkSkippedMessage = "Se omitieron {0} propiedades no elegibles.";

    private void ClearSelection() => SelectedIds.Clear();

    private List<ListingSummaryDto> GetSelectedListings() =>
        Listings.Where(listing => SelectedIds.Contains(listing.Id)).ToList();

    private static bool HasImages(ListingSummaryDto listing) => listing.ImageCount > 0;

    private static bool CanMoveToDraft(ListingSummaryDto listing) =>
        listing.Status == ListingStatus.Published || listing.Status == ListingStatus.Portfolio;

    private static bool CanUnarchive(ListingSummaryDto listing) =>
        listing.Status == ListingStatus.Archived;

    private static bool CanArchive(ListingSummaryDto listing) =>
        listing.Status != ListingStatus.Archived;

    private static bool CanPublish(ListingSummaryDto listing) =>
        listing.Status != ListingStatus.Published && HasImages(listing);

    private static bool CanMoveToPortfolio(ListingSummaryDto listing) =>
        listing.Status != ListingStatus.Portfolio && HasImages(listing);

    private async Task BulkPublish()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(CanPublish).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Publicar propiedades",
            $"¿Publicar {eligible.Count} propiedades seleccionadas?",
            eligible,
            id => ListingService.PublishAsync(new PublishListingDto { ListingId = id }),
            count => $"{count} propiedades publicadas",
            skipped);
    }

    private async Task BulkArchive()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(CanArchive).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Archivar propiedades",
            $"¿Archivar {eligible.Count} propiedades seleccionadas?",
            eligible,
            id => ListingService.ArchiveAsync(id),
            count => $"{count} propiedades archivadas",
            skipped);
    }

    private async Task BulkMoveToPortfolio()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(CanMoveToPortfolio).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Mover a Portafolio",
            $"¿Mover {eligible.Count} propiedades al portafolio?",
            eligible,
            id => PortfolioService.CreateFromListingAsync(id),
            count => $"{count} propiedades movidas al portafolio",
            skipped);
    }

    private async Task BulkUnpublish()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(CanMoveToDraft).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Mover a borrador",
            $"¿Mover {eligible.Count} propiedades a borrador?",
            eligible,
            id => ListingService.UnpublishAsync(id),
            count => $"{count} propiedades enviadas a borrador",
            skipped);
    }

    private async Task BulkUnarchive()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(CanUnarchive).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Desarchivar propiedades",
            $"¿Desarchivar {eligible.Count} propiedades seleccionadas?",
            eligible,
            id => ListingService.UnarchiveAsync(id),
            count => $"{count} propiedades desarchivadas",
            skipped);
    }

    private async Task BulkFeature()
    {
        var selected = GetSelectedListings();
        var eligible = selected
            .Where(listing => !IsFeatured(listing.Id) && CanToggleFeatured(listing))
            .ToList();

        if (!eligible.Any())
        {
            Snackbar.Add("No hay propiedades elegibles para destacar.", Severity.Warning);
            return;
        }

        var availableSlots = Math.Max(0, MaxFeaturedListings - FeaturedListingCount);
        if (availableSlots == 0)
        {
            Snackbar.Add(string.Format(L["Admin:Listings:Featured:LimitReached"], MaxFeaturedListings), Severity.Warning);
            return;
        }

        var toFeature = eligible.Take(availableSlots).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - toFeature.Count;

        await ConfirmAndExecuteBulk(
            "Destacar propiedades",
            $"¿Destacar {toFeature.Count} propiedades seleccionadas?",
            toFeature,
            id => FeaturedListingService.AddAsync(new CreateFeaturedListingDto { ListingId = id }),
            count => $"{count} propiedades destacadas",
            skipped);
    }

    private async Task BulkUnfeature()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Where(listing => IsFeatured(listing.Id)).Select(listing => listing.Id).ToList();
        var skipped = selected.Count - eligible.Count;

        await ConfirmAndExecuteBulk(
            "Quitar destacados",
            $"¿Quitar destacado a {eligible.Count} propiedades?",
            eligible,
            id => FeaturedListingService.RemoveByListingIdAsync(id),
            count => $"{count} propiedades actualizadas",
            skipped);
    }

    private async Task BulkDelete()
    {
        var selected = GetSelectedListings();
        var eligible = selected.Select(listing => listing.Id).ToList();

        await ConfirmAndExecuteBulk(
            "Eliminar propiedades",
            $"¿Eliminar {eligible.Count} propiedades? Esta acción no se puede deshacer.",
            eligible,
            id => ListingService.DeleteAsync(id),
            count => $"{count} propiedades eliminadas",
            skippedCount: 0);
    }

    private async Task ConfirmAndExecuteBulk(
        string title,
        string message,
        IReadOnlyCollection<Guid> ids,
        Func<Guid, Task> action,
        Func<int, string> successMessage,
        int skippedCount = 0)
    {
        if (!ids.Any())
        {
            Snackbar.Add("No hay propiedades elegibles para esta acción.", Severity.Warning);
            return;
        }

        var confirmed = await DialogService.ShowMessageBox(title, message, yesText: "Confirmar", cancelText: "Cancelar");
        if (confirmed != true)
        {
            return;
        }

        var success = 0;
        var failed = 0;
        foreach (var id in ids.ToList())
        {
            try
            {
                await action(id);
                success++;
            }
            catch
            {
                failed++;
            }
        }

        Snackbar.Add(successMessage(success), Severity.Success);

        if (skippedCount > 0)
        {
            Snackbar.Add(string.Format(BulkSkippedMessage, skippedCount), Severity.Warning);
        }

        if (failed > 0)
        {
            Snackbar.Add($"No se pudieron procesar {failed} propiedades.", Severity.Warning);
        }

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

    private async Task MoveListingToPortfolio(ListingSummaryDto listing) =>
        await ExecuteWithReload(async () =>
            {
                await PortfolioService.CreateFromListingAsync(listing.Id);
                await SuggestSaleRegistrationAsync(listing);
            },
            "Propiedad movida al portafolio");

    private async Task SuggestSaleRegistrationAsync(ListingSummaryDto listing)
    {
        var confirm = await DialogService.ShowMessageBox(
            L["Sales:Dialog:SuggestTitle"],
            L["Sales:Dialog:SuggestPortfolio"],
            yesText: L["Sales:Dialog:Open"],
            cancelText: L["Common:Cancel"]);

        if (confirm == true)
        {
            await OpenSaleDialog(listing);
        }
    }

    private async Task DuplicateListing(Guid id) =>
        await ExecuteWithReload(() => ListingService.DuplicateAsync(id), "Propiedad duplicada exitosamente");

    private async Task OpenSaleDialog(ListingSummaryDto listing)
    {
        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            Snackbar.Add(L["Sales:Validation:StatusNotAllowed"], Severity.Warning);
            return;
        }

        var parameters = new DialogParameters
        {
            { nameof(ListingSaleDialog.ListingId), listing.Id },
            { nameof(ListingSaleDialog.ListingTitle), listing.Title },
            { nameof(ListingSaleDialog.DefaultAmount), listing.Price },
            { nameof(ListingSaleDialog.DefaultCurrency), "MXN" }
        };

        var dialog = await DialogService.ShowAsync<ListingSaleDialog>(
            L["Sales:Dialog:Open"],
            parameters,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });

        var result = await dialog.Result;
        if (result?.Data is ListingSaleDto)
        {
            Snackbar.Add(L["Sales:Dialog:Saved"], Severity.Success);
            await LoadListings();
        }
        else if (result?.Data is bool)
        {
            Snackbar.Add(L["Sales:Dialog:Deleted"], Severity.Success);
            await LoadListings();
        }
    }

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





using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.ContactRequests;
using cima.Domain.Shared;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace cima.Blazor.Client.Pages.Admin.ContactRequests;

public partial class Index : cimaComponentBase
{
    [Inject] private IContactRequestAppService ContactRequestService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private IReadOnlyList<ContactRequestDto> Requests { get; set; } = new List<ContactRequestDto>();
    private int TotalCount { get; set; }
    private bool IsLoading { get; set; } = true;
    
    // Pagination
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            IsLoading = true;
            // Note: The AppService method signature in contracts is (skipCount, maxResultCount)
            // But we might want to filter or sort. For now, basic pagination.
            var result = await ContactRequestService.GetListAsync(
                (CurrentPage - 1) * PageSize,
                PageSize
            );
            
            Requests = result.Items;
            TotalCount = (int)result.TotalCount;
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["Common:ErrorLoadingData"], Severity.Error);
            Console.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        CurrentPage = page;
        await LoadData();
    }

    private Color GetStatusColor(ContactRequestStatus status) => status switch
    {
        ContactRequestStatus.New => Color.Info,
        ContactRequestStatus.Read => Color.Warning,
        ContactRequestStatus.Replied => Color.Success,
        ContactRequestStatus.Closed => Color.Default,
        _ => Color.Default
    };

    private async Task MarkAsReplied(ContactRequestDto request)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", "¿Marcar esta solicitud como respondida?" },
            { "ButtonText", "Confirmar" },
            { "Color", Color.Success }
        };

        // TODO: Create a specialized dialog for replying with notes if needed.
        // For now, using a simple prompt or just marking it.
        // The MarkAsRepliedDto requires ReplyNotes. Let's ask for them.
        
        // Simple input dialog using MudBlazor
        var options = new DialogOptions { CloseOnEscapeKey = true };
    }
    
    // Implement Full View / Reply logic in a separate method or Dialog later
    // For MVP, just list and show details.
}

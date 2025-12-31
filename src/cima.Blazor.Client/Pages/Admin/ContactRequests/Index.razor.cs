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

    private async Task HandleMarkAsRead(ContactRequestDto request)
    {
        try
        {
            // Marcar como leído = marcar como respondido con nota vacía
            await ContactRequestService.MarkAsRepliedAsync(request.Id, new MarkAsRepliedDto { ReplyNotes = "Leído" });
            Snackbar.Add(L["Admin:ContactRequests:MarkedAsRead"], Severity.Success);
            await LoadData();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{L["Common:Error"]}: {ex.Message}", Severity.Error);
        }
    }

    private async Task HandleMarkAsReplied(ContactRequestDto request)
    {
        try
        {
            // Por ahora, una nota simple. TODO: Implementar diálogo para notas personalizadas.
            await ContactRequestService.MarkAsRepliedAsync(request.Id, new MarkAsRepliedDto { ReplyNotes = "Respondido por email" });
            Snackbar.Add(L["Admin:ContactRequests:MarkedAsReplied"], Severity.Success);
            await LoadData();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{L["Common:Error"]}: {ex.Message}", Severity.Error);
        }
    }

    private int TotalPages => TotalCount > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadData();
        }
    }

    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadData();
        }
    }

    private async Task HandleStatusChange(ContactRequestDto request, string action)
    {
        if (action == "read")
        {
            await HandleMarkAsRead(request);
        }
        else if (action == "replied")
        {
            await HandleMarkAsReplied(request);
        }
    }
}


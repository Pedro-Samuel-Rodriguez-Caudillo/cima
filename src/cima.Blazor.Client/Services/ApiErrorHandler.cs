using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MudBlazor;
using cima.Localization;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Standard handler for API operations in Admin forms.
/// Manages loading states, error handling, and user feedback.
/// </summary>
public class ApiErrorHandler
{
    private readonly ISnackbar _snackbar;
    private readonly IStringLocalizer<cimaResource> _l;

    public ApiErrorHandler(ISnackbar snackbar, IStringLocalizer<cimaResource> l)
    {
        _snackbar = snackbar;
        _l = l;
    }

    /// <summary>
    /// Executes an async action with standardized error handling.
    /// </summary>
    /// <param name="action">The async operation to execute</param>
    /// <returns>True if successful, False if an exception occurred</returns>
    public async Task<bool> HandleAsync(Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            // TODO: Parse specific AbpValidationException here for friendlier messages (Phase 4)
            _snackbar.Add(_l["Common:Error"], Severity.Error);
            Console.WriteLine(ex); // Log for debugging
            return false;
        }
    }
}

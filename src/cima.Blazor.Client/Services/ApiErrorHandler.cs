using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MudBlazor;
using cima.Localization;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Standard handler for API operations in Admin forms.
/// Manages loading states, error handling, and user feedback.
/// Parses exceptions to show user-friendly messages.
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
    /// <param name="successMessage">Optional success message to show</param>
    /// <returns>True if successful, False if an exception occurred</returns>
    public async Task<bool> HandleAsync(Func<Task> action, string? successMessage = null)
    {
        try
        {
            await action();
            if (!string.IsNullOrEmpty(successMessage))
            {
                _snackbar.Add(successMessage, Severity.Success);
            }
            return true;
        }
        catch (HttpRequestException httpEx)
        {
            HandleHttpException(httpEx);
            return false;
        }
        catch (Exception ex)
        {
            HandleGenericException(ex);
            return false;
        }
    }

    /// <summary>
    /// Executes an async function with standardized error handling.
    /// </summary>
    public async Task<T?> HandleAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException httpEx)
        {
            HandleHttpException(httpEx);
            return default;
        }
        catch (Exception ex)
        {
            HandleGenericException(ex);
            return default;
        }
    }

    private void HandleHttpException(HttpRequestException ex)
    {
        var statusCode = ex.StatusCode;
        
        if (statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _snackbar.Add(_l["Error:Unauthorized"], Severity.Warning);
        }
        else if (statusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _snackbar.Add(_l["Error:Forbidden"], Severity.Warning);
        }
        else if (statusCode == System.Net.HttpStatusCode.NotFound)
        {
            _snackbar.Add(_l["Error:NotFound"], Severity.Warning);
        }
        else if (statusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Errores de validacion - mostrar mensaje detallado si está disponible
            var errorMessage = ExtractValidationMessage(ex.Message);
            _snackbar.Add(errorMessage ?? _l["Error:ValidationDetails"], Severity.Warning);
        }
        else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            _snackbar.Add(_l["Common:ServerError"], Severity.Error);
        }
        else
        {
            _snackbar.Add(_l["Error:NetworkConnection"], Severity.Error);
        }

        Console.WriteLine($"HTTP error ({statusCode}): {ex.Message}");
    }

    private void HandleGenericException(Exception ex)
    {
        var message = ex.Message;
        var exTypeName = ex.GetType().Name;

        // Detectar excepciones de ABP por nombre de tipo
        if (exTypeName.Contains("BusinessException") || 
            exTypeName.Contains("UserFriendlyException"))
        {
            // Mostrar mensaje directo para excepciones amigables
            _snackbar.Add(message, Severity.Warning);
        }
        else if (exTypeName.Contains("ValidationException") ||
                 message.Contains("validation", StringComparison.OrdinalIgnoreCase))
        {
            // Mostrar errores de validación detallados
            var errorMessage = ExtractValidationMessage(message);
            _snackbar.Add(errorMessage ?? _l["Error:ValidationDetails"], Severity.Warning);
        }
        // Detectar errores de conexion
        else if (message.Contains("fetch", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("Failed to fetch", StringComparison.OrdinalIgnoreCase))
        {
            _snackbar.Add(_l["Error:NetworkConnection"], Severity.Error);
        }
        else if (message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("401", StringComparison.OrdinalIgnoreCase))
        {
            _snackbar.Add(_l["Error:Unauthorized"], Severity.Warning);
        }
        else if (message.Contains("forbidden", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("403", StringComparison.OrdinalIgnoreCase))
        {
            _snackbar.Add(_l["Error:Forbidden"], Severity.Warning);
        }
        else if (message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("404", StringComparison.OrdinalIgnoreCase))
        {
            _snackbar.Add(_l["Error:NotFound"], Severity.Warning);
        }
        else
        {
            // Error generico con mensaje original si es corto
            if (message.Length < 100 && !message.Contains("Exception"))
            {
                _snackbar.Add(message, Severity.Error);
            }
            else
            {
                _snackbar.Add(_l["Common:Error"], Severity.Error);
            }
        }

        Console.WriteLine($"Error ({exTypeName}): {ex}");
    }

    /// <summary>
    /// Extracts user-friendly validation messages from exception messages.
    /// Handles ABP validation exception formats and common error patterns.
    /// </summary>
    private string? ExtractValidationMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        // Si el mensaje es corto y legible (no es un stack trace o JSON), usarlo directamente
        if (message.Length < 200 && 
            !message.Contains("Exception") && 
            !message.StartsWith("{") && 
            !message.Contains("at ") &&
            !message.Contains("StackTrace"))
        {
            return message;
        }

        // Intentar extraer mensajes de error de respuestas de ABP/JSON
        // Patrón típico: "error": { "message": "..." }
        var messageMatch = System.Text.RegularExpressions.Regex.Match(
            message, 
            @"""message""\s*:\s*""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (messageMatch.Success)
        {
            return messageMatch.Groups[1].Value;
        }

        // Patrón para validationErrors de ABP
        var validationMatch = System.Text.RegularExpressions.Regex.Match(
            message,
            @"""validationErrors""\s*:\s*\[([^\]]+)\]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (validationMatch.Success)
        {
            // Extraer los mensajes de error individuales
            var errorsSection = validationMatch.Groups[1].Value;
            var errorMessages = System.Text.RegularExpressions.Regex.Matches(
                errorsSection,
                @"""message""\s*:\s*""([^""]+)""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (errorMessages.Count > 0)
            {
                var messages = new System.Collections.Generic.List<string>();
                foreach (System.Text.RegularExpressions.Match match in errorMessages)
                {
                    messages.Add(match.Groups[1].Value);
                }
                return string.Join(". ", messages);
            }
        }

        return null;
    }

    /// <summary>
    /// Shows a success message
    /// </summary>
    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success);
    }

    /// <summary>
    /// Shows an info message
    /// </summary>
    public void ShowInfo(string message)
    {
        _snackbar.Add(message, Severity.Info);
    }

    /// <summary>
    /// Shows a warning message
    /// </summary>
    public void ShowWarning(string message)
    {
        _snackbar.Add(message, Severity.Warning);
    }

    /// <summary>
    /// Shows an error message
    /// </summary>
    public void ShowError(string message)
    {
        _snackbar.Add(message, Severity.Error);
    }
}

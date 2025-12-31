using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Gdpr;

/// <summary>
/// Servicio para manejar solicitudes GDPR (Art. 17 y 20).
/// Permite exportar y eliminar datos personales de usuarios.
/// </summary>
public interface IGdprRequestService : IApplicationService
{
    /// <summary>
    /// Exporta todos los datos personales del usuario actual (GDPR Art. 20 - Portabilidad)
    /// </summary>
    /// <returns>Archivo JSON con todos los datos del usuario</returns>
    Task<GdprExportResultDto> ExportMyDataAsync();

    /// <summary>
    /// Solicita la eliminación de la cuenta del usuario actual (GDPR Art. 17 - Derecho al olvido)
    /// </summary>
    Task RequestAccountDeletionAsync(DeleteAccountRequestDto input);

    /// <summary>
    /// Obtiene el estado de una solicitud de eliminación pendiente
    /// </summary>
    Task<DeletionRequestStatusDto?> GetDeletionRequestStatusAsync();

    /// <summary>
    /// Cancela una solicitud de eliminación pendiente
    /// </summary>
    Task CancelDeletionRequestAsync();
}

/// <summary>
/// Resultado de la exportación de datos GDPR
/// </summary>
public class GdprExportResultDto
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
}

/// <summary>
/// Solicitud de eliminación de cuenta
/// </summary>
public class DeleteAccountRequestDto
{
    /// <summary>
    /// Razón de la eliminación (opcional)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Confirmación de que el usuario entiende las consecuencias
    /// </summary>
    public bool ConfirmDeletion { get; set; }
}

/// <summary>
/// Estado de una solicitud de eliminación
/// </summary>
public class DeletionRequestStatusDto
{
    public DateTime RequestedAt { get; set; }
    public DateTime ScheduledDeletionAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanBeCancelled { get; set; }
}

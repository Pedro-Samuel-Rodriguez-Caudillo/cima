using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using cima.ContactRequests;
using cima.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace cima.Gdpr;

/// <summary>
/// Servicio para manejar solicitudes GDPR.
/// Permite a los usuarios exportar y eliminar sus datos personales.
/// </summary>
[Authorize]
public class GdprRequestAppService : ApplicationService, IGdprRequestService
{
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentUser _currentUser;
    private const string DeletionRequestedAtKey = "GdprDeletionRequestedAt";
    private const string DeletionScheduledAtKey = "GdprDeletionScheduledAt";
    private const string DeletionStatusKey = "GdprDeletionStatus";
    private const string DeletionReasonKey = "GdprDeletionReason";
    private const string DeletionCancelledAtKey = "GdprDeletionCancelledAt";
    private const string DeletionCompletedAtKey = "GdprDeletionCompletedAt";
    private const string DeletionStatusPending = "Pending";
    private const string DeletionStatusCancelled = "Cancelled";
    private const string DeletionStatusCompleted = "Completed";
    private static readonly TimeSpan DeletionGracePeriod = TimeSpan.FromDays(30);

    public GdprRequestAppService(
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        ICurrentUser currentUser)
    {
        _contactRequestRepository = contactRequestRepository;
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Exporta todos los datos personales del usuario actual (GDPR Art. 20)
    /// </summary>
    public async Task<GdprExportResultDto> ExportMyDataAsync()
    {
        var userId = _currentUser.GetId();
        Logger.LogInformation("Procesando solicitud de exportacion GDPR para usuario {UserId}", userId);

        var exportData = new Dictionary<string, object>();

        // 1. Datos basicos del usuario
        var user = await _userRepository.GetAsync(userId);
        exportData["user"] = new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.Name,
            user.Surname,
            user.CreationTime,
            user.LastModificationTime
        };

        // 2. Perfil de arquitecto (si existe)
        var architect = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == userId);
        if (architect != null)
        {
            exportData["architectProfile"] = new
            {
                architect.Id,
                architect.UserId,
                architect.TotalListingsPublished,
                architect.ActiveListings,
                architect.RegistrationDate,
                architect.IsActive
            };


            // 3. Propiedades del arquitecto
            var listings = await _listingRepository
                .GetListAsync(l => l.ArchitectId == architect.Id);
            
            exportData["listings"] = listings.Select(l => new
            {
                l.Id,
                l.Title,
                l.Description,
                l.Status,
                l.Price,
                l.CreatedAt
            }).ToList();


        }

        // 4. Solicitudes de contacto (como remitente por email)
        var userEmail = user.Email;
        if (!string.IsNullOrEmpty(userEmail))
        {
            var contactRequests = await _contactRequestRepository
                .GetListAsync(cr => cr.Email == userEmail);
            
            exportData["contactRequests"] = contactRequests.Select(cr => new
            {
                cr.Id,
                cr.Name,
                cr.Email,
                cr.Phone,
                cr.Message,
                cr.Status,
                cr.CreatedAt
            }).ToList();
        }

        // 5. Metadata de exportacion
        exportData["exportMetadata"] = new
        {
            ExportedAt = Clock.Now,
            ExportedBy = userId,
            DataFormat = "JSON",
            GdprArticle = "Art. 20 - Right to Data Portability"
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonContent = JsonSerializer.Serialize(exportData, jsonOptions);
        var fileContent = Encoding.UTF8.GetBytes(jsonContent);

        Logger.LogInformation("Exportacion GDPR completada para usuario {UserId}. Tamano: {Size} bytes", userId, fileContent.Length);

        return new GdprExportResultDto
        {
            FileContent = fileContent,
            FileName = $"cima-data-export-{userId}-{Clock.Now:yyyyMMdd}.json",
            ContentType = "application/json"
        };
    }

    /// <summary>
    /// Solicita la eliminacion de la cuenta (GDPR Art. 17)
    /// Nota: La eliminacion real requiere un proceso administrativo
    /// </summary>
    public async Task RequestAccountDeletionAsync(DeleteAccountRequestDto input)
    {
        if (!input.ConfirmDeletion)
        {
            throw new UserFriendlyException("Debe confirmar que desea eliminar su cuenta");
        }

        var userId = _currentUser.GetId();
        var user = await _userRepository.GetAsync(userId);
        var existingStatus = GetStringExtra(user, DeletionStatusKey);
        if (string.Equals(existingStatus, DeletionStatusPending, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("Ya existe una solicitud de eliminacion pendiente");
        }
        if (string.Equals(existingStatus, DeletionStatusCompleted, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("La cuenta ya fue eliminada");
        }

        var requestedAt = Clock.Now;
        var scheduledAt = requestedAt.Add(DeletionGracePeriod);
        SetExtraProperty(user, DeletionRequestedAtKey, requestedAt);
        SetExtraProperty(user, DeletionScheduledAtKey, scheduledAt);
        SetExtraProperty(user, DeletionStatusKey, DeletionStatusPending);
        SetExtraProperty(user, DeletionReasonKey, input.Reason);
        SetExtraProperty(user, DeletionCancelledAtKey, null);
        SetExtraProperty(user, DeletionCompletedAtKey, null);
        await _userRepository.UpdateAsync(user, autoSave: true);

        Logger.LogWarning(
            "Solicitud de eliminacion GDPR recibida para usuario {UserId}. Razon: {Reason}",
            userId,
            input.Reason ?? "No especificada");

        // Registrar la solicitud (en produccion, esto deberia guardarse en una tabla dedicada)
        // Por ahora, solo registramos en logs para procesamiento manual
        Logger.LogCritical(
            "GDPR_DELETION_REQUEST: UserId={UserId}, RequestedAt={RequestedAt}, Reason={Reason}",
            userId,
            Clock.Now,
            input.Reason);

        await ExecuteAccountDeletionAsync(user, input.Reason);

        Logger.LogInformation(
            "GDPR deletion completada para usuario {UserId}",
            userId);
    }

    /// <summary>
    /// Obtiene el estado de una solicitud de eliminacion pendiente
    /// </summary>
    public Task<DeletionRequestStatusDto?> GetDeletionRequestStatusAsync()
    {
        return GetDeletionRequestStatusInternalAsync();
    }

    /// <summary>
    /// Cancela una solicitud de eliminacion pendiente
    /// </summary>
    public Task CancelDeletionRequestAsync()
    {
        return CancelDeletionRequestInternalAsync();
    }

    #region Metodos de anonimizacion (helpers)

    /// <summary>
    /// Anonimiza email para cumplimiento GDPR
    /// </summary>
    public static string AnonymizeEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            return "***@***.***";
        }

        var parts = email.Split('@');
        var localPart = parts[0].Length > 2
            ? parts[0][..2] + new string('*', parts[0].Length - 2)
            : "***";

        return $"{localPart}@***.***";
    }

    /// <summary>
    /// Anonimiza telefono para cumplimiento GDPR
    /// </summary>
    public static string AnonymizePhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
        {
            return "***";
        }

        return new string('*', phone.Length - 4) + phone[^4..];
    }

    #endregion

    private async Task<DeletionRequestStatusDto?> GetDeletionRequestStatusInternalAsync()
    {
        var userId = _currentUser.GetId();
        var user = await _userRepository.GetAsync(userId);

        var requestedAt = GetDateTimeExtra(user, DeletionRequestedAtKey);
        if (requestedAt == null)
        {
            return null;
        }

        var scheduledAt = GetDateTimeExtra(user, DeletionScheduledAtKey) ?? requestedAt.Value.Add(DeletionGracePeriod);
        var status = GetStringExtra(user, DeletionStatusKey) ?? DeletionStatusPending;
        var canBeCancelled = string.Equals(status, DeletionStatusPending, StringComparison.OrdinalIgnoreCase)
                             && Clock.Now < scheduledAt;

        return new DeletionRequestStatusDto
        {
            RequestedAt = requestedAt.Value,
            ScheduledDeletionAt = scheduledAt,
            Status = status,
            CanBeCancelled = canBeCancelled
        };
    }

    private async Task CancelDeletionRequestInternalAsync()
    {
        var userId = _currentUser.GetId();
        var user = await _userRepository.GetAsync(userId);
        var requestedAt = GetDateTimeExtra(user, DeletionRequestedAtKey);
        if (requestedAt == null)
        {
            throw new UserFriendlyException("No hay solicitud de eliminacion pendiente");
        }

        var status = GetStringExtra(user, DeletionStatusKey) ?? DeletionStatusPending;
        if (!string.Equals(status, DeletionStatusPending, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("La solicitud de eliminacion no puede cancelarse");
        }

        SetExtraProperty(user, DeletionStatusKey, DeletionStatusCancelled);
        SetExtraProperty(user, DeletionCancelledAtKey, Clock.Now);
        await _userRepository.UpdateAsync(user, autoSave: true);

        Logger.LogInformation("Solicitud GDPR cancelada para usuario {UserId}", userId);
    }

    private static void SetExtraProperty(IdentityUser user, string key, object? value)
    {
        if (value == null)
        {
            user.ExtraProperties.Remove(key);
            return;
        }

        user.ExtraProperties[key] = value;
    }

    private static string? GetStringExtra(IdentityUser user, string key)
    {
        if (!user.ExtraProperties.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        return value.ToString();
    }

    private static DateTime? GetDateTimeExtra(IdentityUser user, string key)
    {
        if (!user.ExtraProperties.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.UtcDateTime;
        }

        if (value is string text && DateTime.TryParse(text, out var parsed))
        {
            return parsed;
        }

        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(element.GetString(), out var parsedElement))
            {
                return parsedElement;
            }
        }

        return null;
    }

    private async Task ExecuteAccountDeletionAsync(IdentityUser user, string? reason)
    {
        var originalEmail = user.Email;

        if (!string.IsNullOrWhiteSpace(originalEmail))
        {
            var contactRequests = await _contactRequestRepository.GetListAsync(cr => cr.Email == originalEmail);
            foreach (var request in contactRequests)
            {
                request.Name = "Anonimo";
                request.Email = AnonymizeEmail(request.Email);
                request.Phone = string.IsNullOrWhiteSpace(request.Phone)
                    ? null
                    : AnonymizePhone(request.Phone);
                request.Message = "[GDPR] Datos personales eliminados";
                await _contactRequestRepository.UpdateAsync(request, autoSave: true);
            }
        }

        var anonymizedSuffix = user.Id.ToString("N")[..8];
        var anonymizedUserName = $"deleted-{anonymizedSuffix}";
        var anonymizedEmail = $"{anonymizedUserName}@example.invalid";

        var nameResult = await _userManager.SetUserNameAsync(user, anonymizedUserName);
        if (!nameResult.Succeeded)
        {
            throw new UserFriendlyException("No se pudo anonimizar el nombre de usuario");
        }

        var emailResult = await _userManager.SetEmailAsync(user, anonymizedEmail);
        if (!emailResult.Succeeded)
        {
            throw new UserFriendlyException("No se pudo anonimizar el email del usuario");
        }

        user.Name = "Deleted";
        user.Surname = "User";

        // Use UserManager methods for protected setters
        await _userManager.SetPhoneNumberAsync(user, null);
        user.SetIsActive(false);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new UserFriendlyException("No se pudo completar la eliminacion de la cuenta");
        }

        SetExtraProperty(user, DeletionStatusKey, DeletionStatusCompleted);
        SetExtraProperty(user, DeletionScheduledAtKey, Clock.Now);
        SetExtraProperty(user, DeletionCompletedAtKey, Clock.Now);
        SetExtraProperty(user, DeletionReasonKey, reason);
        await _userRepository.UpdateAsync(user, autoSave: true);
    }
}

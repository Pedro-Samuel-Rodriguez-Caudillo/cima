using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.BackgroundJobs;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Notifications;
using cima.Permissions;
using cima.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace cima.ContactRequests;

public class ContactRequestAppService : cimaAppService, IContactRequestAppService
{
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IRepository<Listing, Guid> _propertyRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;

    public ContactRequestAppService(
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IRepository<Listing, Guid> propertyRepository,
        IRepository<Architect, Guid> architectRepository,
        IBackgroundJobManager backgroundJobManager,
        ISettingProvider settingProvider,
        IConfiguration configuration)
    {
        _contactRequestRepository = contactRequestRepository;
        _propertyRepository = propertyRepository;
        _architectRepository = architectRepository;
        _backgroundJobManager = backgroundJobManager;
        _settingProvider = settingProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// PUBLICO: Cualquiera puede crear solicitud (sin login)
    /// </summary>
    [AllowAnonymous]
    public async Task<ContactRequestDto> CreateAsync(CreateContactRequestDto input)
    {
        // Normalizar datos de entrada
        var normalizedName = input.Name?.Trim();
        var normalizedEmail = input.Email?.Trim().ToLowerInvariant();
        var normalizedPhone = input.Phone?.Trim();
        var normalizedMessage = input.Message?.Trim();

        // Validaciones adicionales (ABP ya valida DataAnnotations)
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new BusinessException("ContactRequest:NameRequired")
                .WithData("Field", "Name");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new BusinessException("ContactRequest:EmailRequired")
                .WithData("Field", "Email");
        }

        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            throw new BusinessException("ContactRequest:MessageRequired")
                .WithData("Field", "Message");
        }

        // VALIDACION: Propiedad existe?
        var property = await _propertyRepository.FindAsync(input.ListingId);
        if (property == null)
        {
            throw new BusinessException("Listing:NotFound")
                .WithData("ListingId", input.ListingId);
        }

        // VALIDACION: Propiedad debe estar Published o en Portfolio
        if (property.Status != ListingStatus.Published && property.Status != ListingStatus.Portfolio)
        {
            throw new BusinessException("ContactRequest:ListingNotAvailable")
                .WithData("ListingId", input.ListingId)
                .WithData("CurrentStatus", property.Status)
                .WithData("RequiredStatus", $"{ListingStatus.Published} or {ListingStatus.Portfolio}");
        }

        var contactRequest = new ContactRequest
        {
            ListingId = input.ListingId,
            Name = normalizedName,
            Email = normalizedEmail,
            Phone = normalizedPhone ?? string.Empty,
            Message = normalizedMessage,
            ArchitectId = property.ArchitectId,
            CreatedAt = Clock.Now,
            Status = ContactRequestStatus.New,
            ReplyNotes = string.Empty
        };

        await _contactRequestRepository.InsertAsync(contactRequest);

        // Encolar job para envío asíncrono de emails (con reintentos automáticos)
        var baseUrl = _configuration["App:SelfUrl"] ?? "https://4cima.com";
        var adminEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.AdminNotificationEmail)
            ?? _configuration["Email:AdminNotification"]
            ?? _configuration["Email:Smtp:FromAddress"];

        if (!string.IsNullOrEmpty(adminEmail))
        {
            await _backgroundJobManager.EnqueueAsync(new ContactEmailJobArgs
            {
                AdminEmail = adminEmail,
                CustomerName = normalizedName!,
                CustomerEmail = normalizedEmail!,
                CustomerPhone = normalizedPhone,
                Message = normalizedMessage!,
                PropertyTitle = property.Title,
                PropertyUrl = $"{baseUrl}/properties/{property.Id}",
                SendConfirmationToCustomer = true
            });
        }

        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    /// <summary>
    /// PUBLICO: Crea solicitud de contacto general (sin propiedad específica)
    /// </summary>
    [AllowAnonymous]
    public async Task<ContactRequestDto> CreateGeneralAsync(CreateGeneralContactRequestDto input)
    {
        var normalizedName = input.Name?.Trim();
        var normalizedEmail = input.Email?.Trim().ToLowerInvariant();
        var normalizedPhone = input.Phone?.Trim();
        var normalizedMessage = input.Message?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new BusinessException("ContactRequest:NameRequired")
                .WithData("Field", "Name");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new BusinessException("ContactRequest:EmailRequired")
                .WithData("Field", "Email");
        }

        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            throw new BusinessException("ContactRequest:MessageRequired")
                .WithData("Field", "Message");
        }

        // Para contacto general, usamos null como ListingId y ArchitectId
        var contactRequest = new ContactRequest
        {
            ListingId = null,
            Name = normalizedName,
            Email = normalizedEmail,
            Phone = normalizedPhone ?? string.Empty,
            Message = normalizedMessage,
            ArchitectId = null,
            CreatedAt = Clock.Now,
            Status = ContactRequestStatus.New,
            ReplyNotes = string.Empty
        };

        await _contactRequestRepository.InsertAsync(contactRequest);

        // Encolar job para envío asíncrono de emails
        var baseUrl = _configuration["App:SelfUrl"] ?? "https://4cima.com";
        var adminEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.AdminNotificationEmail)
            ?? _configuration["Email:AdminNotification"]
            ?? _configuration["Email:Smtp:FromAddress"];

        if (!string.IsNullOrEmpty(adminEmail))
        {
            await _backgroundJobManager.EnqueueAsync(new ContactEmailJobArgs
            {
                AdminEmail = adminEmail,
                CustomerName = normalizedName!,
                CustomerEmail = normalizedEmail!,
                CustomerPhone = normalizedPhone,
                Message = normalizedMessage!,
                PropertyTitle = "Consulta General",
                PropertyUrl = $"{baseUrl}/contact",
                SendConfirmationToCustomer = true
            });
        }

        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }


    [Authorize(cimaPermissions.ContactRequests.View)]
    public async Task<PagedResultDto<ContactRequestDto>> GetListAsync(
        int skipCount, int maxResultCount)
    {
        var queryable = await _contactRequestRepository.GetQueryableAsync();
        queryable = queryable.OrderByDescending(cr => cr.CreatedAt);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var requests = await AsyncExecuter.ToListAsync(
            queryable.Skip(skipCount).Take(maxResultCount)
        );

        return new PagedResultDto<ContactRequestDto>(
            totalCount,
            ObjectMapper.Map<List<ContactRequest>, List<ContactRequestDto>>(requests)
        );
    }

    [Authorize(cimaPermissions.ContactRequests.View)]
    public async Task<PagedResultDto<ContactRequestDto>> GetByArchitectAsync(
        Guid architectId, int skipCount, int maxResultCount)
    {
        var queryable = await _contactRequestRepository.GetQueryableAsync();
        queryable = queryable
            .Where(cr => cr.ArchitectId == architectId)
            .OrderByDescending(cr => cr.CreatedAt);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var requests = await AsyncExecuter.ToListAsync(
            queryable.Skip(skipCount).Take(maxResultCount)
        );

        return new PagedResultDto<ContactRequestDto>(
            totalCount,
            ObjectMapper.Map<List<ContactRequest>, List<ContactRequestDto>>(requests)
        );
    }

    [Authorize(cimaPermissions.ContactRequests.Reply)]
    public async Task<ContactRequestDto> MarkAsRepliedAsync(Guid id, MarkAsRepliedDto input)
    {
        var contactRequest = await _contactRequestRepository.GetAsync(id);

        // VALIDACION: Solo el arquitecto dueno puede responder (o admin para contacto general)
        await ValidateArchitectOwnershipAsync(contactRequest.ArchitectId, "responder");

        contactRequest.Status = ContactRequestStatus.Replied;
        contactRequest.ReplyNotes = input.ReplyNotes;
        contactRequest.RepliedAt = Clock.Now;

        await _contactRequestRepository.UpdateAsync(contactRequest);
        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    [Authorize(cimaPermissions.ContactRequests.Close)]
    public async Task<ContactRequestDto> CloseAsync(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetAsync(id);

        await ValidateArchitectOwnershipAsync(contactRequest.ArchitectId, "cerrar");

        contactRequest.Status = ContactRequestStatus.Closed;

        await _contactRequestRepository.UpdateAsync(contactRequest);
        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    [Authorize(cimaPermissions.ContactRequests.View)]
    public async Task<ContactRequestDto> GetAsync(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetAsync(id);
        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    /// <summary>
    /// Valida que el usuario actual sea el propietario del arquitecto o un administrador.
    /// Para contacto general (architectId == null), solo admins pueden gestionar.
    /// </summary>
    private async Task ValidateArchitectOwnershipAsync(Guid? architectId, string operationName)
    {
        // Si es contacto general (sin arquitecto), solo admins pueden gestionar
        if (architectId == null)
        {
            if (!CurrentUser.IsInRole("admin"))
            {
                throw new AbpAuthorizationException($"Solo administradores pueden {operationName} solicitudes generales");
            }
            return;
        }

        var architect = await _architectRepository.GetAsync(architectId.Value);
        if (architect.UserId != CurrentUser.Id && !CurrentUser.IsInRole("admin"))
        {
            throw new AbpAuthorizationException($"Solo puedes {operationName} solicitudes de tus propiedades");
        }
    }
}

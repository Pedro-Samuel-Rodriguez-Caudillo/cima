using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Notifications;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace cima.ContactRequests;

public class ContactRequestAppService : cimaAppService, IContactRequestAppService
{
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IRepository<Listing, Guid> _propertyRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IEmailNotificationService _emailService;
    private readonly IConfiguration _configuration;

    public ContactRequestAppService(
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IRepository<Listing, Guid> propertyRepository,
        IRepository<Architect, Guid> architectRepository,
        IEmailNotificationService emailService,
        IConfiguration configuration)
    {
        _contactRequestRepository = contactRequestRepository;
        _propertyRepository = propertyRepository;
        _architectRepository = architectRepository;
        _emailService = emailService;
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

        // Enviar notificaciones por email (fire and forget)
        _ = SendNotificationsAsync(contactRequest, property, normalizedName!, normalizedEmail!, normalizedMessage!);

        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    /// <summary>
    /// Envía notificaciones por email de forma asíncrona
    /// </summary>
    private async Task SendNotificationsAsync(
        ContactRequest contactRequest, 
        Listing property,
        string customerName,
        string customerEmail,
        string message)
    {
        try
        {
            var baseUrl = _configuration["App:SelfUrl"] ?? "https://4cima.com";
            var adminEmail = _configuration["Email:AdminNotification"] ?? _configuration["Email:Smtp:FromAddress"];

            // Notificar al admin
            if (!string.IsNullOrEmpty(adminEmail))
            {
                await _emailService.SendContactRequestNotificationAsync(new ContactRequestNotificationDto
                {
                    AdminEmail = adminEmail,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    CustomerPhone = contactRequest.Phone,
                    Message = message,
                    PropertyTitle = property.Title,
                    PropertyUrl = $"{baseUrl}/properties/{property.Id}"
                });
            }

            // Confirmar al cliente
            await _emailService.SendContactRequestConfirmationAsync(new ContactRequestConfirmationDto
            {
                CustomerEmail = customerEmail,
                CustomerName = customerName,
                PropertyTitle = property.Title
            });
        }
        catch (Exception ex)
        {
            // Log pero no interrumpir el flujo
            Logger.LogWarning(ex, "Error al enviar notificaciones de solicitud de contacto {ContactRequestId}", contactRequest.Id);
        }
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

        // VALIDACION: Solo el arquitecto dueno puede responder
        var architect = await _architectRepository.GetAsync(contactRequest.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !CurrentUser.IsInRole("admin"))
        {
            throw new AbpAuthorizationException(
                "Solo puedes responder solicitudes de tus propiedades");
        }

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

        var architect = await _architectRepository.GetAsync(contactRequest.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !CurrentUser.IsInRole("admin"))
        {
            throw new AbpAuthorizationException(
                "Solo puedes cerrar solicitudes de tus propiedades");
        }

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
}

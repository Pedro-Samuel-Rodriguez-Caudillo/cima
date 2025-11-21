using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
namespace cima.ContactRequests;

public  class ContactRequestAppService : cimaAppService, IContactRequestAppService
{
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IRepository<Listing, Guid> _propertyRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ContactRequestAppService(
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IRepository<Listing, Guid> propertyRepository,
        IRepository<Architect, Guid> architectRepository)
    {
        _contactRequestRepository = contactRequestRepository;
        _propertyRepository = propertyRepository;
        _architectRepository = architectRepository;
    }

    /// <summary>
    /// PUBLICO: Cualquiera puede crear solicitud (sin login)
    /// </summary>
    [AllowAnonymous]
    public async Task<ContactRequestDto> CreateAsync(CreateContactRequestDto input)
    {
        // VALIDACION: Propiedad existe?
        var property = await _propertyRepository.FindAsync(input.ListingId);
        if (property == null)
        {
            throw new BusinessException("Listing:NotFound")
                .WithData("ListingId", input.ListingId);
        }

        var contactRequest = new ContactRequest
        {
            ListingId = input.ListingId,
            Name = input.Name,
            Email = input.Email,
            Phone = input.Phone ?? string.Empty,
            Message = input.Message,
            ArchitectId = property.ArchitectId,
            CreatedAt = Clock.Now,
            Status = ContactRequestStatus.New,
            ReplyNotes = string.Empty
        };

        await _contactRequestRepository.InsertAsync(contactRequest);
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

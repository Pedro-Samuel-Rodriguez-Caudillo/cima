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
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace cima.Services
{
    /// <summary>
    /// Implementacion del servicio de propiedades inmobiliarias
    /// </summary>
    [Authorize(cimaPermissions.Properties.Default)]
    public class PropertyAppService : cimaAppService, IPropertyAppService
    {
        private readonly IRepository<Property, Guid> _propertyRepository;
        private readonly IRepository<Architect, Guid> _architectRepository;

        public PropertyAppService(
            IRepository<Property, Guid> propertyRepository,
            IRepository<Architect, Guid> architectRepository)
        {
            _propertyRepository = propertyRepository;
            _architectRepository = architectRepository;
        }

        /// <summary>
        /// Obtiene lista paginada de propiedades con filtros
        /// </summary>
        public async Task<PagedResultDto<PropertyDto>> GetListAsync(PropertyFiltersDto filters)
        {
            var queryable = await _propertyRepository.GetQueryableAsync();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                queryable = queryable.Where(p =>
                    p.Title.Contains(filters.SearchTerm) ||
                    p.Location.Contains(filters.SearchTerm) ||
                    p.Description.Contains(filters.SearchTerm));
            }

            if (filters.Status.HasValue)
            {
                queryable = queryable.Where(p => p.Status == filters.Status.Value);
            }

            if (filters.MinPrice.HasValue)
            {
                queryable = queryable.Where(p => p.Price >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                queryable = queryable.Where(p => p.Price <= filters.MaxPrice.Value);
            }

            if (filters.MinBedrooms.HasValue)
            {
                queryable = queryable.Where(p => p.Bedrooms >= filters.MinBedrooms.Value);
            }

            if (filters.MinBathrooms.HasValue)
            {
                queryable = queryable.Where(p => p.Bathrooms >= filters.MinBathrooms.Value);
            }

            if (filters.ArchitectId.HasValue)
            {
                queryable = queryable.Where(p => p.ArchitectId == filters.ArchitectId.Value);
            }

            // Aplicar ordenamiento
            queryable = filters.SortBy?.ToLower() switch
            {
                "price" => filters.SortDescending
                    ? queryable.OrderByDescending(p => p.Price)
                    : queryable.OrderBy(p => p.Price),
                "area" => filters.SortDescending
                    ? queryable.OrderByDescending(p => p.Area)
                    : queryable.OrderBy(p => p.Area),
                "createdat" => filters.SortDescending
                    ? queryable.OrderByDescending(p => p.CreatedAt)
                    : queryable.OrderBy(p => p.CreatedAt),
                _ => queryable.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            var properties = await AsyncExecuter.ToListAsync(
                queryable
                    .Skip(filters.SkipCount)
                    .Take(filters.MaxResultCount)
            );

            return new PagedResultDto<PropertyDto>(
                totalCount,
                ObjectMapper.Map<List<Property>, List<PropertyDto>>(properties)
            );
        }

        /// <summary>
        /// Obtiene detalle de una propiedad por Id
        /// </summary>
        public async Task<PropertyDto> GetAsync(Guid id)
        {
            var property = await _propertyRepository.GetAsync(id);
            return ObjectMapper.Map<Property, PropertyDto>(property);
        }

        /// <summary>
        /// Crea nueva propiedad en estado Draft
        /// </summary>
        [Authorize(cimaPermissions.Properties.Create)]
        public async Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input)
        {
            // Validar que el arquitecto existe
            var architectExists = await _architectRepository.AnyAsync(a => a.Id == input.ArchitectId);
            if (!architectExists)
            {
                throw new BusinessException("Architect:NotFound")
                    .WithData("ArchitectId", input.ArchitectId);
            }

            var property = ObjectMapper.Map<CreateUpdatePropertyDto, Property>(input);
            property.CreatedAt = Clock.Now;
            property.CreatedBy = CurrentUser.Id;
            property.Status = PropertyStatus.Draft;

            await _propertyRepository.InsertAsync(property);
            return ObjectMapper.Map<Property, PropertyDto>(property);
        }

        /// <summary>
        /// Actualiza propiedad existente
        /// </summary>
        [Authorize(cimaPermissions.Properties.Edit)]
        public async Task<PropertyDto> UpdateAsync(Guid id, CreateUpdatePropertyDto input)
        {
            var property = await _propertyRepository.GetAsync(id);

            // Validacion: Solo el dueno o admin puede editar
            var architect = await _architectRepository.GetAsync(property.ArchitectId);
            if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
            {
                throw new AbpAuthorizationException("Solo puedes editar tus propias propiedades");
            }

            ObjectMapper.Map(input, property);
            property.LastModifiedAt = Clock.Now;
            property.LastModifiedBy = CurrentUser.Id;

            await _propertyRepository.UpdateAsync(property);
            return ObjectMapper.Map<Property, PropertyDto>(property);
        }

        /// <summary>
        /// Elimina propiedad
        /// </summary>
        [Authorize(cimaPermissions.Properties.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var property = await _propertyRepository.GetAsync(id);

            // Validacion: Solo el dueno o admin puede eliminar
            var architect = await _architectRepository.GetAsync(property.ArchitectId);
            if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
            {
                throw new AbpAuthorizationException("Solo puedes eliminar tus propias propiedades");
            }

            await _propertyRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Cambia estado de Draft a Published
        /// Requiere al menos 1 imagen
        /// </summary>
        [Authorize(cimaPermissions.Properties.Publish)]
        public async Task<PropertyDto> PublishAsync(Guid id)
        {
            var property = await _propertyRepository.GetAsync(id);

            // Validacion: Debe tener al menos 1 imagen
            if (property.Images == null || !property.Images.Any())
            {
                throw new BusinessException("Property:RequiresImages")
                    .WithData("Message", "No se puede publicar sin imagenes");
            }

            property.Status = PropertyStatus.Published;
            property.LastModifiedAt = Clock.Now;
            property.LastModifiedBy = CurrentUser.Id;

            await _propertyRepository.UpdateAsync(property);
            return ObjectMapper.Map<Property, PropertyDto>(property);
        }

        /// <summary>
        /// Cambia estado a Archived
        /// </summary>
        [Authorize(cimaPermissions.Properties.Archive)]
        public async Task<PropertyDto> ArchiveAsync(Guid id)
        {
            var property = await _propertyRepository.GetAsync(id);

            property.Status = PropertyStatus.Archived;
            property.LastModifiedAt = Clock.Now;
            property.LastModifiedBy = CurrentUser.Id;

            await _propertyRepository.UpdateAsync(property);
            return ObjectMapper.Map<Property, PropertyDto>(property);
        }

        /// <summary>
        /// Obtiene propiedades de un arquitecto especifico
        /// </summary>
        public async Task<PagedResultDto<PropertyDto>> GetByArchitectAsync(
            Guid architectId, int skipCount, int maxResultCount)
        {
            var queryable = await _propertyRepository.GetQueryableAsync();
            queryable = queryable.Where(p => p.ArchitectId == architectId);

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var properties = await AsyncExecuter.ToListAsync(
                queryable
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip(skipCount)
                    .Take(maxResultCount)
            );

            return new PagedResultDto<PropertyDto>(
                totalCount,
                ObjectMapper.Map<List<Property>, List<PropertyDto>>(properties)
            );
        }

        /// <summary>
        /// Verifica si el usuario actual es administrador
        /// </summary>
        private async Task<bool> IsAdminAsync()
        {
            return await Task.FromResult(CurrentUser.IsInRole("admin"));
        }
    }
}

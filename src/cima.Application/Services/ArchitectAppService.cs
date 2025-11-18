using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace cima.Services
{
    [Authorize(cimaPermissions.Architects.Default)]
    public class ArchitectAppService : cimaAppService, IArchitectAppService
    {
        private readonly IRepository<Architect, Guid> _architectRepository;

        public ArchitectAppService(IRepository<Architect, Guid> architectRepository)
        {
            _architectRepository = architectRepository;
        }

        public async Task<ArchitectDto> GetAsync(Guid id)
        {
            var architect = await _architectRepository.GetAsync(id);
            return ObjectMapper.Map<Architect, ArchitectDto>(architect);
        }

        public async Task<ArchitectDto> GetByUserIdAsync(Guid userId)
        {
            var architect = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == userId);

            if (architect == null)
            {
                throw new BusinessException("Architect:NotFound")
                    .WithData("UserId", userId);
            }

            return ObjectMapper.Map<Architect, ArchitectDto>(architect);
        }

        [Authorize(cimaPermissions.Architects.Create)]
        public async Task<ArchitectDto> CreateAsync(CreateArchitectDto input)
        {
            // VALIDACION: Usuario ya tiene perfil de arquitecto?
            var existingArchitect = await _architectRepository
                .FirstOrDefaultAsync(a => a.UserId == CurrentUser.Id.Value);

            if (existingArchitect != null)
            {
                throw new BusinessException("Architect:AlreadyExists")
                    .WithData("UserId", CurrentUser.Id);
            }

            var architect = ObjectMapper.Map<CreateArchitectDto, Architect>(input);
            architect.UserId = CurrentUser.Id.Value;  // Asignar usuario actual

            await _architectRepository.InsertAsync(architect);
            return ObjectMapper.Map<Architect, ArchitectDto>(architect);

        }

        [Authorize(cimaPermissions.Architects.Edit)]
        public async Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input)
        {
            var architect = await _architectRepository.GetAsync(id);

            // VALIDACION: Es el dueno?
            if (architect.UserId != CurrentUser.Id && !CurrentUser.IsInRole("admin"))
            {
                throw new AbpAuthorizationException("Solo puedes editar tu propio perfil");
            }

            ObjectMapper.Map(input, architect);
            await _architectRepository.UpdateAsync(architect);

            return ObjectMapper.Map<Architect, ArchitectDto>(architect);

        }

        [Authorize(cimaPermissions.Architects.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var architect = await _architectRepository.GetAsync(id);

            // VALIDACION: Es el dueno?
            if (architect.UserId != CurrentUser.Id && !CurrentUser.IsInRole("admin"))
            {
                throw new AbpAuthorizationException("Solo puedes eliminar tu propio perfil");
            }

            await _architectRepository.DeleteAsync(id);
        }
    }
}
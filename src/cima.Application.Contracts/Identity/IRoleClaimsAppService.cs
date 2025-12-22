using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Identity;

public interface IRoleClaimsAppService : IApplicationService
{
    Task<ListResultDto<RoleClaimDto>> GetListAsync(Guid roleId);
    Task UpdateAsync(Guid roleId, UpdateRoleClaimsInput input);
}

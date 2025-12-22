using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace cima.Identity;

public interface IRoleMembersAppService : IApplicationService
{
    Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetRoleMembersInput input);
}

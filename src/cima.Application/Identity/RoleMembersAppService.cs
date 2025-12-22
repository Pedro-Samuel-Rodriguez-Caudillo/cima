using System.Collections.Generic;
using System.Threading.Tasks;
using cima.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Identity;

namespace cima.Identity;

[Authorize(IdentityPermissions.Users.Default)]
public class RoleMembersAppService : ApplicationService, IRoleMembersAppService
{
    private readonly IIdentityUserRepository _userRepository;

    public RoleMembersAppService(
        IIdentityUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetRoleMembersInput input)
    {
        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(IdentityUser.CreationTime)} DESC"
            : input.Sorting;

        var totalCount = await _userRepository.GetCountAsync(roleId: input.RoleId);
        var users = await _userRepository.GetListAsync(
            sorting: sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            roleId: input.RoleId);

        var dtos = ObjectMapper.Map<List<IdentityUser>, List<IdentityUserDto>>(users);
        return new PagedResultDto<IdentityUserDto>(totalCount, dtos);
    }
}

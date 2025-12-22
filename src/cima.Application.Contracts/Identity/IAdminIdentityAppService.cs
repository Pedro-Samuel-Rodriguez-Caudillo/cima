using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Identity;

public interface IAdminIdentityAppService : IApplicationService
{
    Task<string> ResetPasswordAsync(Guid id, ResetIdentityUserPasswordDto input, CancellationToken cancellationToken = default);
}

using System.Collections.Generic;

namespace cima.Identity;

public class UpdateRoleClaimsInput
{
    public List<RoleClaimDto> Claims { get; set; } = new();
}

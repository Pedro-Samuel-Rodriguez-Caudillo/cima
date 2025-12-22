using System;
using Volo.Abp.Application.Dtos;

namespace cima.Identity;

public class GetRoleMembersInput : PagedAndSortedResultRequestDto
{
    public Guid RoleId { get; set; }
}

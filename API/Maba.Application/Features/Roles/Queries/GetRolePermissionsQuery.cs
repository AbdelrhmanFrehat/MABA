using MediatR;
using Maba.Application.Features.Roles.DTOs;

namespace Maba.Application.Features.Roles.Queries;

public class GetRolePermissionsQuery : IRequest<List<PermissionDto>>
{
    public Guid RoleId { get; set; }
}


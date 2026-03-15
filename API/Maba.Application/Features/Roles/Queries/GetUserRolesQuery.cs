using MediatR;
using Maba.Application.Features.Roles.DTOs;

namespace Maba.Application.Features.Roles.Queries;

public class GetUserRolesQuery : IRequest<List<RoleDto>>
{
    public Guid UserId { get; set; }
}


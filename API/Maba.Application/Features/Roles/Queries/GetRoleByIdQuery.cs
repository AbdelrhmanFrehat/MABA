using MediatR;
using Maba.Application.Features.Roles.DTOs;

namespace Maba.Application.Features.Roles.Queries;

public class GetRoleByIdQuery : IRequest<RoleDto>
{
    public Guid Id { get; set; }
}


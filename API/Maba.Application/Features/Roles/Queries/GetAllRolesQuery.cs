using MediatR;
using Maba.Application.Features.Roles.DTOs;

namespace Maba.Application.Features.Roles.Queries;

public class GetAllRolesQuery : IRequest<List<RoleDto>>
{
}


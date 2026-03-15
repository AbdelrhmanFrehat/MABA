using MediatR;
using Maba.Application.Features.Users.DTOs;

namespace Maba.Application.Features.Users.Queries;

public class GetUsersByRoleQuery : IRequest<List<UserDto>>
{
    public Guid RoleId { get; set; }
    public bool? IsActive { get; set; }
}


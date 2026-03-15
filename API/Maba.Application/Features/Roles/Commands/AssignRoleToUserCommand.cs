using MediatR;

namespace Maba.Application.Features.Roles.Commands;

public class AssignRoleToUserCommand : IRequest<Unit>
{
    public Guid RoleId { get; set; }
    public Guid UserId { get; set; }
}


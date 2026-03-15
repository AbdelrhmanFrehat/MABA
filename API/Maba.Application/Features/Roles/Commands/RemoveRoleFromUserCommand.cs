using MediatR;

namespace Maba.Application.Features.Roles.Commands;

public class RemoveRoleFromUserCommand : IRequest<Unit>
{
    public Guid RoleId { get; set; }
    public Guid UserId { get; set; }
}


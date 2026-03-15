using MediatR;

namespace Maba.Application.Features.Roles.Commands;

public class RemovePermissionFromRoleCommand : IRequest<Unit>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}


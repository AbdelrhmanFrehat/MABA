using MediatR;
using Maba.Application.Features.Roles.DTOs;

namespace Maba.Application.Features.Roles.Commands;

public class UpdateRoleCommand : IRequest<RoleDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}


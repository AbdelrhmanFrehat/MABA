using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public AssignPermissionToRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if assignment already exists
        var existing = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (existing != null)
        {
            return Unit.Value; // Already assigned
        }

        // Verify role and permission exist
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            throw new KeyNotFoundException("Role not found.");
        }

        var permission = await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            throw new KeyNotFoundException("Permission not found.");
        }

        // Create assignment
        var rolePermission = new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId
        };

        _context.Set<RolePermission>().Add(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


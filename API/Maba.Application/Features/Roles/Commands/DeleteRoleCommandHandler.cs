using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        // Remove role permissions
        var rolePermissions = role.RolePermissions.ToList();
        foreach (var rp in rolePermissions)
        {
            _context.Set<RolePermission>().Remove(rp);
        }

        // Remove user roles
        var userRoles = role.UserRoles.ToList();
        foreach (var ur in userRoles)
        {
            _context.Set<UserRole>().Remove(ur);
        }

        // Remove role
        _context.Set<Role>().Remove(role);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


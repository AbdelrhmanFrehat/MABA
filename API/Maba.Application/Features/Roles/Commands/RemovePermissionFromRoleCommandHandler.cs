using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RemovePermissionFromRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        var rolePermission = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (rolePermission == null)
        {
            throw new KeyNotFoundException("Permission assignment not found.");
        }

        _context.Set<RolePermission>().Remove(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


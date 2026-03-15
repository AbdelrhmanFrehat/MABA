using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var userRole = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (userRole == null)
        {
            throw new KeyNotFoundException("Role assignment not found.");
        }

        // Check if this is the last admin role (prevent removing last admin)
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role != null && role.Name == "Admin")
        {
            var adminUsers = await _context.Set<UserRole>()
                .Where(ur => ur.RoleId == request.RoleId)
                .CountAsync(cancellationToken);

            if (adminUsers <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last admin role.");
            }
        }

        _context.Set<UserRole>().Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}


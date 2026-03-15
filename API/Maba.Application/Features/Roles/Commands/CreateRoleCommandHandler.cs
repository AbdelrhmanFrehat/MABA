using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Application.Features.Roles.DTOs;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var existingRole = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == request.Name, cancellationToken);

        if (existingRole != null)
        {
            throw new InvalidOperationException("Role with this name already exists");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        _context.Set<Role>().Add(role);

        // Add permissions
        if (request.PermissionIds.Any())
        {
            var permissions = await _context.Set<Permission>()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                _context.Set<RolePermission>().Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load role with permissions for response
        var roleWithPermissions = await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

        return new RoleDto
        {
            Id = roleWithPermissions!.Id,
            Name = roleWithPermissions.Name,
            Description = roleWithPermissions.Description,
            Permissions = roleWithPermissions.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Key = rp.Permission.Key,
                Name = rp.Permission.Name,
                CreatedAt = rp.Permission.CreatedAt,
                UpdatedAt = rp.Permission.UpdatedAt
            }).ToList(),
            CreatedAt = roleWithPermissions.CreatedAt,
            UpdatedAt = roleWithPermissions.UpdatedAt
        };
    }
}


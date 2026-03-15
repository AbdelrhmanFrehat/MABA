using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Commands;
using Maba.Application.Features.Roles.DTOs;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        // Check if name is being changed and if it conflicts
        if (role.Name != request.Name)
        {
            var existingRole = await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == request.Name && r.Id != request.Id, cancellationToken);

            if (existingRole != null)
            {
                throw new InvalidOperationException("Role with this name already exists");
            }
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        // Update permissions
        var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
        var permissionsToAdd = request.PermissionIds.Except(existingPermissionIds).ToList();
        var permissionsToRemove = existingPermissionIds.Except(request.PermissionIds).ToList();

        // Remove permissions
        if (permissionsToRemove.Any())
        {
            var rolePermissionsToRemove = role.RolePermissions
                .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                .ToList();

            foreach (var rp in rolePermissionsToRemove)
            {
                _context.Set<RolePermission>().Remove(rp);
            }
        }

        // Add new permissions
        if (permissionsToAdd.Any())
        {
            var permissions = await _context.Set<Permission>()
                .Where(p => permissionsToAdd.Contains(p.Id))
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


using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.Queries;
using Maba.Application.Features.Roles.DTOs;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == request.UserId)
            .Select(ur => ur.Role)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystemRole = r.IsSystemRole,
            Priority = r.Priority,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }
}


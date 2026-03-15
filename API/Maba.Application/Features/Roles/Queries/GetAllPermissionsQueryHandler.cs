using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Roles.DTOs;
using Maba.Application.Features.Roles.Queries;
using Maba.Domain.Users;

namespace Maba.Application.Features.Roles.Handlers;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Set<Permission>()
            .ToListAsync(cancellationToken);

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Key = p.Key,
            Name = p.Name,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}


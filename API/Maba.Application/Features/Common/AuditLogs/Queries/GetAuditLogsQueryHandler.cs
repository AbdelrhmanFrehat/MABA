using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Common.AuditLogs.Queries;
using Maba.Application.Features.Common.AuditLogs.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.AuditLogs.Handlers;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<AuditLog>()
            .Include(al => al.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(al => al.EntityType == request.EntityType);
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(al => al.EntityId == request.EntityId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(al => al.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(al => al.Action == request.Action);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(al => al.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(al => al.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var auditLogs = await query
            .OrderByDescending(al => al.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var auditLogsDto = auditLogs.Select(al => new AuditLogDto
        {
            Id = al.Id,
            EntityType = al.EntityType,
            EntityId = al.EntityId,
            Action = al.Action,
            OldValues = al.OldValues,
            NewValues = al.NewValues,
            UserId = al.UserId,
            UserEmail = al.UserEmail ?? al.User?.Email,
            UserFullName = al.User?.FullName,
            IpAddress = al.IpAddress,
            UserAgent = al.UserAgent,
            RequestPath = al.RequestPath,
            RequestMethod = al.RequestMethod,
            StatusCode = al.StatusCode,
            ErrorMessage = al.ErrorMessage,
            DurationMs = al.DurationMs,
            CreatedAt = al.CreatedAt
        }).ToList();

        return new PagedResult<AuditLogDto>
        {
            Items = auditLogsDto,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}


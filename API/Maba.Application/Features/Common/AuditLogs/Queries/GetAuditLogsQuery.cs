using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Common.AuditLogs.DTOs;

namespace Maba.Application.Features.Common.AuditLogs.Queries;

public class GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}


using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;

    public AuditService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string entityType, Guid? entityId, string action, Guid? userId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? requestPath = null, string? requestMethod = null, int? statusCode = null, string? errorMessage = null, long? durationMs = null, CancellationToken cancellationToken = default)
    {
        var userEmail = userId.HasValue
            ? await _context.Set<User>()
                .Where(u => u.Id == userId.Value)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RequestPath = requestPath,
            RequestMethod = requestMethod,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            DurationMs = durationMs
        };

        _context.Set<AuditLog>().Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


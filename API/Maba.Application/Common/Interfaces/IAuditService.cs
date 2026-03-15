namespace Maba.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogActionAsync(string entityType, Guid? entityId, string action, Guid? userId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? requestPath = null, string? requestMethod = null, int? statusCode = null, string? errorMessage = null, long? durationMs = null, CancellationToken cancellationToken = default);
}


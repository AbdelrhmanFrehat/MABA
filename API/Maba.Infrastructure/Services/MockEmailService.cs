using Maba.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maba.Infrastructure.Services;

/// <summary>
/// No-op email implementation when SMTP is not configured. Logs at debug level.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendRequestConfirmationAsync(
        string? customerEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send confirmation to {Email} for {ReferenceNumber} ({RequestType})",
            customerEmail ?? "(none)",
            referenceNumber,
            requestTypeLabel);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string verificationLink, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "SMTP is not configured (Smtp:Host is empty). Verification email was NOT sent to {Email}. To send real emails, set Smtp:Host, Smtp:Username, Smtp:Password etc. in User Secrets or appsettings.",
            toEmail);
        return Task.CompletedTask;
    }
}

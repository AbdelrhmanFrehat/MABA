using Maba.Application.Common.Emails;
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
        string? viewRequestUrl,
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

    public Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "SMTP is not configured (Smtp:Host is empty). Password reset email was NOT sent to {Email}. To send real emails, set Smtp:Host, Smtp:Username, Smtp:Password etc. in User Secrets or appsettings.",
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendShopOrderConfirmationAsync(
        string? toEmail,
        ShopOrderConfirmationEmailModel model,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send shop order confirmation to {Email} for order {OrderNumber}",
            toEmail ?? "(none)",
            model.OrderNumber);
        return Task.CompletedTask;
    }

    public Task SendShopOrderShippedAsync(
        string? toEmail,
        ShopOrderShippedEmailModel model,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send shop shipped email to {Email} for order {OrderNumber}",
            toEmail ?? "(none)",
            model.OrderNumber);
        return Task.CompletedTask;
    }

    public Task SendShopOrderCancelledAsync(
        string? toEmail,
        ShopOrderCancelledEmailModel model,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send shop order cancelled to {Email} for order {OrderNumber}",
            toEmail ?? "(none)",
            model.OrderNumber);
        return Task.CompletedTask;
    }

    public Task SendRequestStatusUpdateAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string newStatus,
        string? viewRequestUrl,
        string? rejectionReason = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send status update ({Status}) to {Email} for {ReferenceNumber} ({RequestType})",
            newStatus, toEmail ?? "(none)", referenceNumber, requestTypeLabel);
        return Task.CompletedTask;
    }

    public Task SendRequestCancelledAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        string? reasonOrNote,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would send request cancelled to {Email} for {ReferenceNumber} ({RequestType})",
            toEmail ?? "(none)",
            referenceNumber,
            requestTypeLabel);
        return Task.CompletedTask;
    }

    public Task SendDirectAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Mock email: would send '{Subject}' to {Email}.", subject, toEmail);
        return Task.CompletedTask;
    }

    public Task SendSupportChatNotificationAsync(
        string? customerName,
        string? customerEmail,
        string subject,
        string? initialMessage,
        string adminChatUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Mock email: would notify admin of new support conversation from {Customer} ({Email}), subject: {Subject}",
            customerName ?? "(unknown)",
            customerEmail ?? "(none)",
            subject);
        return Task.CompletedTask;
    }
}

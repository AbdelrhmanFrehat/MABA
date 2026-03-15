using System.Net;
using System.Net.Mail;
using System.Text;
using Maba.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maba.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> options, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendRequestConfirmationAsync(
        string? customerEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                await SendCustomerConfirmationAsync(customerEmail, customerName, referenceNumber, requestTypeLabel, cancellationToken);
            }
            else
            {
                _logger.LogDebug("No customer email provided; skipping confirmation email for {ReferenceNumber}", referenceNumber);
            }

            if (!string.IsNullOrWhiteSpace(_settings.NotificationToEmail))
            {
                await SendInternalNotificationAsync(customerName, referenceNumber, requestTypeLabel, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send request confirmation email for {ReferenceNumber}", referenceNumber);
        }
    }

    public async Task SendEmailVerificationAsync(string toEmail, string verificationLink, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Verify your email address";
            var body = new StringBuilder();
            body.AppendLine("Please verify your email address by clicking the link below:");
            body.AppendLine();
            body.AppendLine(verificationLink);
            body.AppendLine();
            body.AppendLine("This link will expire in 24 hours.");
            body.AppendLine();
            body.AppendLine("If you did not create an account, you can ignore this email.");
            body.AppendLine();
            body.AppendLine("Thank you,");
            body.AppendLine(_settings.FromName);
            await SendAsync(toEmail, subject, body.ToString(), cancellationToken);
            _logger.LogInformation("Verification email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email verification to {Email}", toEmail);
        }
    }

    private async Task SendCustomerConfirmationAsync(
        string toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        CancellationToken cancellationToken)
    {
        var displayName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
        var subject = $"Request received – {referenceNumber}";
        var body = new StringBuilder();
        body.AppendLine($"Dear {displayName},");
        body.AppendLine();
        body.AppendLine($"We have received your {requestTypeLabel} (reference {referenceNumber}).");
        body.AppendLine();
        body.AppendLine("MABA will review your request and respond soon.");
        body.AppendLine();
        body.AppendLine("Thank you,");
        body.AppendLine(_settings.FromName);

        await SendAsync(toEmail, subject, body.ToString(), cancellationToken);
    }

    private async Task SendInternalNotificationAsync(
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        CancellationToken cancellationToken)
    {
        var subject = $"New request – {referenceNumber}";
        var body = $"New {requestTypeLabel} request {referenceNumber} from {customerName ?? "unknown"}.";
        await SendAsync(_settings.NotificationToEmail!, subject, body, cancellationToken);
    }

    private async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        using var client = CreateSmtpClient();
        var message = new MailMessage
        {
            From = string.IsNullOrWhiteSpace(_settings.FromName)
                ? new MailAddress(_settings.FromEmail)
                : new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_settings.Host, _settings.Port);
        client.EnableSsl = _settings.Port == 465 || _settings.Port == 587;
        if (!string.IsNullOrEmpty(_settings.Username))
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }
        return client;
    }
}

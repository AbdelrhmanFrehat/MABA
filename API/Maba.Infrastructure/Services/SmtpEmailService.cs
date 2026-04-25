using System.Net;
using System.Net.Mail;
using System.Text;
using Maba.Application.Common.Emails;
using Maba.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maba.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IOptions<SmtpSettings> options, IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendRequestConfirmationAsync(
        string? customerEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                await SendCustomerConfirmationAsync(customerEmail, customerName, referenceNumber, requestTypeLabel, viewRequestUrl, cancellationToken);
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
        var subject = "Verify your email address";
        var body = BuildEmailTemplate(
            title: "Verify your email",
            message: "Welcome to MABA. Please confirm your email address to activate your account and start using the platform.",
            actionText: "Verify Email",
            actionUrl: verificationLink,
            secondaryText: "This verification link expires in 24 hours. If you did not create this account, you can safely ignore this email."
        );
        await SendAsync(toEmail, subject, body, cancellationToken);
        _logger.LogInformation("Verification email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Reset your password";
        var body = BuildEmailTemplate(
            title: "Reset your password",
            message: "We received a request to reset your password. Click the button below to choose a new secure password.",
            actionText: "Reset Password",
            actionUrl: resetLink,
            secondaryText: "This reset link expires in 24 hours. If you did not request a password reset, you can safely ignore this email."
        );
        await SendAsync(toEmail, subject, body, cancellationToken);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendShopOrderConfirmationAsync(
        string? toEmail,
        ShopOrderConfirmationEmailModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogDebug("No customer email; skipping order confirmation for order {OrderNumber}", model.OrderNumber);
                return;
            }

            var subject = $"Order confirmed – {model.OrderNumber}";
            var body = ShopOrderEmailHtmlBuilder.BuildConfirmationHtml(model);
            await SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogInformation("Shop order confirmation email sent to {Email} for {OrderNumber}", toEmail, model.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send shop order confirmation for {OrderNumber}", model.OrderNumber);
        }
    }

    public async Task SendShopOrderShippedAsync(
        string? toEmail,
        ShopOrderShippedEmailModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogDebug("No customer email; skipping shipped email for order {OrderNumber}", model.OrderNumber);
                return;
            }

            var subject = $"Your order has been shipped – {model.OrderNumber}";
            var body = ShopOrderEmailHtmlBuilder.BuildShippedHtml(model);
            await SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogInformation("Shop order shipped email sent to {Email} for {OrderNumber}", toEmail, model.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send shop shipped email for {OrderNumber}", model.OrderNumber);
        }
    }

    public async Task SendShopOrderCancelledAsync(
        string? toEmail,
        ShopOrderCancelledEmailModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogDebug("No customer email; skipping order cancelled email for order {OrderNumber}", model.OrderNumber);
                return;
            }

            var subject = $"Order cancelled – {model.OrderNumber}";
            var body = ShopOrderEmailHtmlBuilder.BuildCancelledHtml(model);
            await SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogInformation("Shop order cancelled email sent to {Email} for {OrderNumber}", toEmail, model.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send shop order cancelled email for {OrderNumber}", model.OrderNumber);
        }
    }

    public async Task SendSupportChatNotificationAsync(
        string? customerName,
        string? customerEmail,
        string subject,
        string? initialMessage,
        string adminChatUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.NotificationToEmail))
            {
                _logger.LogDebug("NotificationToEmail not configured; skipping support chat notification.");
                return;
            }

            var name = string.IsNullOrWhiteSpace(customerName) ? "A customer" : WebUtility.HtmlEncode(customerName.Trim());
            var email = string.IsNullOrWhiteSpace(customerEmail) ? "" : $" ({WebUtility.HtmlEncode(customerEmail.Trim())})";
            var safeSubject = WebUtility.HtmlEncode(subject.Trim());
            var messagePreview = string.IsNullOrWhiteSpace(initialMessage)
                ? "<em style=\"color:#888\">(no initial message)</em>"
                : $"<blockquote style=\"border-left:3px solid #667eea;margin:0.75rem 0;padding:0.5rem 0.75rem;background:#f0f2ff;border-radius:0 6px 6px 0;\">{WebUtility.HtmlEncode(initialMessage.Length > 400 ? initialMessage[..400] + "…" : initialMessage)}</blockquote>";

            var emailSubject = $"New support request — {subject}";
            var body = BuildEmailTemplate(
                title: "New support conversation",
                message: $"{name}{email} has opened a new support conversation.<br/><br/><strong>Subject:</strong> {safeSubject}<br/><br/>{messagePreview}",
                actionText: "Open Support Chat",
                actionUrl: adminChatUrl,
                secondaryText: "Log in to the admin panel and open Support Chat to reply."
            );

            await SendAsync(_settings.NotificationToEmail, emailSubject, body, cancellationToken);
            _logger.LogInformation("Support chat notification sent for conversation subject: {Subject}", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send support chat notification for subject: {Subject}", subject);
        }
    }

    public async Task SendRequestCancelledAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        string? reasonOrNote,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogDebug("No customer email; skipping request cancelled email for {ReferenceNumber}", referenceNumber);
                return;
            }

            var displayName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
            var safeDisplayName = WebUtility.HtmlEncode(displayName);
            var safeRequestType = WebUtility.HtmlEncode(requestTypeLabel);
            var safeRef = WebUtility.HtmlEncode(referenceNumber);
            var fallbackBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
            var actionUrl = !string.IsNullOrWhiteSpace(viewRequestUrl) ? viewRequestUrl.Trim() : fallbackBase;
            var reasonHtml = string.IsNullOrWhiteSpace(reasonOrNote)
                ? string.Empty
                : $"<br/><br/><strong>Note from our team:</strong><br/>{WebUtility.HtmlEncode(reasonOrNote.Trim())}";

            var message =
                $"Hi {safeDisplayName}, your <strong>{safeRequestType}</strong> request <strong>{safeRef}</strong> has been cancelled.{reasonHtml}<br/><br/>If you have questions or believe this was a mistake, please contact us.";

            var subject = $"Request cancelled – {referenceNumber}";
            var body = BuildEmailTemplate(
                title: "Request cancelled",
                message: message,
                actionText: "Open MABA",
                actionUrl: actionUrl,
                secondaryText: "You can submit a new request anytime from our services pages.");

            await SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogInformation("Request cancelled email sent to {Email} for {ReferenceNumber}", toEmail, referenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send request cancelled email for {ReferenceNumber}", referenceNumber);
        }
    }

    public async Task SendRequestStatusUpdateAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string newStatus,
        string? viewRequestUrl,
        string? rejectionReason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogDebug(
                    "No customer email; skipping status update email for {ReferenceNumber} ({Status})",
                    referenceNumber, newStatus);
                return;
            }

            var displayName  = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
            var safeRef      = WebUtility.HtmlEncode(referenceNumber);
            var safeType     = WebUtility.HtmlEncode(requestTypeLabel);
            var safeDispName = WebUtility.HtmlEncode(displayName);
            var fallbackBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
            var actionUrl    = !string.IsNullOrWhiteSpace(viewRequestUrl) ? viewRequestUrl.Trim() : fallbackBase;

            var (subject, headline, bodyMessage, secondaryText) =
                BuildStatusContent(newStatus, safeDispName, safeRef, safeType, rejectionReason);

            var html = BuildEmailTemplate(
                title: headline,
                message: bodyMessage,
                actionText: "View Request",
                actionUrl: actionUrl,
                secondaryText: secondaryText);

            await SendAsync(toEmail, subject, html, cancellationToken);
            _logger.LogInformation(
                "Status update email ({Status}) sent to {Email} for {ReferenceNumber}",
                newStatus, toEmail, referenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send status update email for {ReferenceNumber} ({Status})",
                referenceNumber, newStatus);
        }
    }

    private static (string subject, string headline, string body, string secondary)
        BuildStatusContent(string status, string name, string refNum, string requestType, string? rejectionReason)
    {
        var safeReason = string.IsNullOrWhiteSpace(rejectionReason)
            ? string.Empty
            : WebUtility.HtmlEncode(rejectionReason.Trim());

        return status.ToLowerInvariant() switch
        {
            "underreview" or "inreview" or "technicalreview" =>
            (
                $"MABA Request Update — {refNum}",
                "Your Request Is Under Review",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> is now under review by our engineering team.<br/><br/>We will assess your request and get back to you as soon as possible.",
                "No action is required from you at this time. We will notify you of any updates."
            ),

            "waitingforinfo" =>
            (
                $"Action Needed for Your MABA Request — {refNum}",
                "We Need More Information",
                $"Hi {name}, our team has reviewed your <strong>{requestType}</strong> request <strong>{refNum}</strong> and requires additional information before we can proceed.<br/><br/>Please log in to your account or contact us to provide the required details.",
                "Providing the requested information promptly will help us process your request faster."
            ),

            "quotationpreparation" =>
            (
                $"MABA Request Update — {refNum}",
                "Quotation Being Prepared",
                $"Hi {name}, our team is currently preparing a detailed quotation for your <strong>{requestType}</strong> request <strong>{refNum}</strong>.<br/><br/>You will receive a follow-up notification once the quotation is ready for your review.",
                "No action is required from you at this time."
            ),

            "quoted" or "quotesent" =>
            (
                $"MABA Request Update — {refNum}",
                "Your Quotation Is Ready",
                $"Hi {name}, we have prepared a quotation for your <strong>{requestType}</strong> request <strong>{refNum}</strong>.<br/><br/>Please log in to your account to review the quotation and let us know how you would like to proceed.",
                "Your quotation will remain valid for a limited time. Please respond at your earliest convenience."
            ),

            "approved" or "accepted" =>
            (
                $"Your MABA Request Has Been Approved — {refNum}",
                "Request Approved",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> has been approved.<br/><br/>Our team is preparing to begin work. You will receive further updates as your request progresses.",
                "If you have any questions, please do not hesitate to contact us."
            ),

            "queued" =>
            (
                $"MABA Request Update — {refNum}",
                "Your Request Is Queued",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> has been queued for processing.<br/><br/>Our team will begin work on it shortly.",
                "You will receive a notification once work begins."
            ),

            "slicing" =>
            (
                $"MABA Request Update — {refNum}",
                "Preparing Your Print",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> is currently being prepared for printing (slicing stage).<br/><br/>Printing will begin shortly.",
                "You will receive a notification once printing starts."
            ),

            "inprogress" or "inexecution" or "printing" =>
            (
                $"MABA Request Update — {refNum}",
                "Your Request Is In Progress",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> is now actively being processed by our engineering team.<br/><br/>We will notify you once it is complete.",
                "No action is required from you at this time."
            ),

            "completed" or "delivered" or "closed" =>
            (
                $"Your MABA Request Has Been Completed — {refNum}",
                "Request Completed",
                $"Hi {name}, your <strong>{requestType}</strong> request <strong>{refNum}</strong> has been completed.<br/><br/>Thank you for choosing MABA Solutions. We look forward to working with you again.",
                "If you have any feedback or follow-up questions, please contact us."
            ),

            "rejected" =>
            (
                $"Your MABA Request Has Been Rejected — {refNum}",
                "Request Not Approved",
                string.IsNullOrWhiteSpace(safeReason)
                    ? $"Hi {name}, after careful review, we were unable to approve your <strong>{requestType}</strong> request <strong>{refNum}</strong>.<br/><br/>We apologise for the inconvenience. Please contact us for more details or to discuss alternatives."
                    : $"Hi {name}, after careful review, we were unable to approve your <strong>{requestType}</strong> request <strong>{refNum}</strong>.<br/><br/><strong>Reason:</strong><br/>{safeReason}<br/><br/>We apologise for the inconvenience. If you have questions or would like to discuss alternatives, please contact us.",
                "You are welcome to submit a revised request or contact our team for further assistance."
            ),

            "failed" =>
            (
                $"MABA Request Update — {refNum}",
                "Request Could Not Be Completed",
                $"Hi {name}, unfortunately your <strong>{requestType}</strong> request <strong>{refNum}</strong> could not be completed at this time.<br/><br/>Our team will be in touch to discuss next steps.",
                "Please contact us if you have any questions or wish to reschedule."
            ),

            _ =>
            (
                $"MABA Request Update — {refNum}",
                "Request Status Updated",
                $"Hi {name}, the status of your <strong>{requestType}</strong> request <strong>{refNum}</strong> has been updated to <strong>{WebUtility.HtmlEncode(status)}</strong>.",
                "Log in to your account to view full details."
            )
        };
    }

    private async Task SendCustomerConfirmationAsync(
        string toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        CancellationToken cancellationToken)
    {
        var displayName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
        var safeDisplayName = WebUtility.HtmlEncode(displayName);
        var safeRequestType = WebUtility.HtmlEncode(requestTypeLabel);
        var subject = $"Request received – {referenceNumber}";
        var fallbackBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
        var actionUrl = !string.IsNullOrWhiteSpace(viewRequestUrl) ? viewRequestUrl.Trim() : fallbackBase;
        var body = BuildEmailTemplate(
            title: "Request received",
            message: $"Hi {safeDisplayName}, we have received your {safeRequestType} request.<br/>Reference Number: <strong>{WebUtility.HtmlEncode(referenceNumber)}</strong>.<br/>Our team will review it and get back to you soon.",
            actionText: "View Request",
            actionUrl: actionUrl,
            secondaryText: "Keep your reference number for follow-up. If this request was not submitted by you, please contact MABA support."
        );

        await SendAsync(toEmail, subject, body, cancellationToken);
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
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
    }

    private string BuildEmailTemplate(
        string title,
        string message,
        string actionText,
        string actionUrl,
        string secondaryText)
    {
        var fromName = string.IsNullOrWhiteSpace(_settings.FromName) ? "MABA" : _settings.FromName;
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeMessage = message;
        var safeActionText = WebUtility.HtmlEncode(actionText);
        var safeActionUrl = WebUtility.HtmlEncode(actionUrl);
        var safeSecondaryText = WebUtility.HtmlEncode(secondaryText);
        var safeFromName = WebUtility.HtmlEncode(fromName);

        return $@"
<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{safeTitle}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f6ff;font-family:Arial,Helvetica,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#f3f6ff;padding:24px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:100%;max-width:600px;background:#ffffff;border-radius:14px;overflow:hidden;"">
          <tr>
            <td style=""padding:22px 28px;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 52%,#16213e 100%);text-align:center;"">
              <img src=""https://mabasol.com/assets/img/logo.jpeg"" alt=""MABA"" width=""46"" height=""46"" style=""display:block;margin:0 auto 10px;border-radius:10px;border:0;"" />
              <div style=""color:#ffffff;font-size:22px;font-weight:700;letter-spacing:0.3px;"">MABA</div>
            </td>
          </tr>
          <tr>
            <td style=""padding:30px 28px 8px 28px;text-align:center;"">
              <h1 style=""margin:0 0 12px 0;font-size:24px;line-height:1.3;color:#121a3f;"">{safeTitle}</h1>
              <p style=""margin:0;font-size:15px;line-height:1.7;color:#46527a;"">{safeMessage}</p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:24px 28px 16px 28px;"">
              <a href=""{safeActionUrl}"" style=""display:inline-block;padding:14px 28px;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#ffffff;text-decoration:none;font-size:16px;font-weight:700;border-radius:10px;"">{safeActionText}</a>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 28px 24px 28px;text-align:center;"">
              <p style=""margin:0;font-size:13px;line-height:1.6;color:#6a7493;"">{safeSecondaryText}</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:18px 28px;background:#f7f9ff;border-top:1px solid #e8edff;text-align:center;"">
              <p style=""margin:0 0 6px 0;font-size:12px;color:#7a84a3;"">{safeFromName} • Engineering Solutions</p>
              <a href=""https://mabasol.com"" style=""font-size:12px;color:#667eea;text-decoration:none;"">mabasol.com</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
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

using System.Net;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Infrastructure.Data;

namespace Maba.Api.Services;

/// <summary>
/// Sends notification emails to every active Admin/Manager user + the Smtp:NotificationToEmail fallback.
/// Uses fire-and-forget per recipient so it never blocks the request. Never throws.
/// </summary>
public class AdminNotificationService
{
    private static readonly HashSet<string> AdminRoleNames =
        new(StringComparer.OrdinalIgnoreCase) { "Admin", "Manager" };

    private readonly ApplicationDbContext _context;
    private readonly IEmailService _email;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        ApplicationDbContext context,
        IEmailService email,
        IConfiguration configuration,
        ILogger<AdminNotificationService> logger)
    {
        _context = context;
        _email = email;
        _configuration = configuration;
        _logger = logger;
    }

    // ─── Public notification methods ──────────────────────────────────────────

    public Task NotifySupportChatAsync(
        string? customerName,
        string? customerEmail,
        string subject,
        string? initialMessage,
        CancellationToken cancellationToken = default)
    {
        var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
        var adminChatUrl = $"{frontendBase}/admin/support-chat";

        var name = string.IsNullOrWhiteSpace(customerName) ? "A customer" : customerName.Trim();
        var email = string.IsNullOrWhiteSpace(customerEmail) ? "" : $" ({customerEmail.Trim()})";
        var previewHtml = string.IsNullOrWhiteSpace(initialMessage)
            ? "<em style=\"color:#888\">(no initial message)</em>"
            : $"<blockquote style=\"border-left:3px solid #667eea;margin:0.75rem 0;padding:0.5rem 0.75rem;background:#f0f2ff;border-radius:0 6px 6px 0;\">{WebUtility.HtmlEncode(initialMessage.Length > 400 ? initialMessage[..400] + "…" : initialMessage)}</blockquote>";

        var emailSubject = $"New support request — {WebUtility.HtmlEncode(subject)}";
        var body = BuildAdminEmail(
            title: "New support conversation",
            body: $"<strong>{WebUtility.HtmlEncode(name)}</strong>{WebUtility.HtmlEncode(email)} has opened a new support conversation.<br/><br/><strong>Subject:</strong> {WebUtility.HtmlEncode(subject)}<br/><br/>{previewHtml}",
            actionText: "Open Support Chat",
            actionUrl: adminChatUrl
        );

        return FanOutAsync(emailSubject, body, cancellationToken);
    }

    public Task NotifyNewRequestAsync(
        string? customerName,
        string? customerEmail,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        CancellationToken cancellationToken = default)
    {
        var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
        var actionUrl = !string.IsNullOrWhiteSpace(viewRequestUrl)
            ? viewRequestUrl.Trim()
            : $"{frontendBase}/admin/requests";

        var name = string.IsNullOrWhiteSpace(customerName) ? "A customer" : customerName.Trim();
        var email = string.IsNullOrWhiteSpace(customerEmail) ? "" : $" ({customerEmail.Trim()})";

        var emailSubject = $"New {requestTypeLabel} — {referenceNumber}";
        var body = BuildAdminEmail(
            title: $"New {WebUtility.HtmlEncode(requestTypeLabel)}",
            body: $"<strong>{WebUtility.HtmlEncode(name)}</strong>{WebUtility.HtmlEncode(email)} has submitted a new <strong>{WebUtility.HtmlEncode(requestTypeLabel)}</strong> request.<br/><br/>Reference: <strong>{WebUtility.HtmlEncode(referenceNumber)}</strong>",
            actionText: "View Request",
            actionUrl: actionUrl
        );

        return FanOutAsync(emailSubject, body, cancellationToken);
    }

    // ─── Core helpers ─────────────────────────────────────────────────────────

    private async Task FanOutAsync(string subject, string htmlBody, CancellationToken cancellationToken)
    {
        List<string> adminEmails;
        try
        {
            adminEmails = await GetAdminEmailsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin emails for notification.");
            return;
        }

        if (adminEmails.Count == 0)
        {
            _logger.LogDebug("No admin emails found; notification skipped.");
            return;
        }

        foreach (var toEmail in adminEmails)
        {
            var capturedEmail = toEmail;
            _ = Task.Run(async () =>
            {
                try { await _email.SendDirectAsync(capturedEmail, subject, htmlBody, CancellationToken.None); }
                catch (Exception ex) { _logger.LogError(ex, "Admin notification failed for {Email}.", capturedEmail); }
            });
        }
    }

    private async Task<List<string>> GetAdminEmailsAsync(CancellationToken cancellationToken)
    {
        var emails = await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .Where(ur =>
                AdminRoleNames.Contains(ur.Role.Name) &&
                ur.User.IsActive &&
                !string.IsNullOrEmpty(ur.User.Email))
            .Select(ur => ur.User.Email)
            .Distinct()
            .ToListAsync(cancellationToken);

        var fallback = _configuration["Smtp:NotificationToEmail"];
        if (!string.IsNullOrWhiteSpace(fallback) &&
            !emails.Any(e => string.Equals(e, fallback, StringComparison.OrdinalIgnoreCase)))
        {
            emails.Add(fallback.Trim());
        }

        return emails;
    }

    private static string BuildAdminEmail(string title, string body, string actionText, string actionUrl)
    {
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeAction = WebUtility.HtmlEncode(actionText);
        var safeUrl = WebUtility.HtmlEncode(actionUrl);

        return $@"<!doctype html>
<html>
<head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1.0""><title>{safeTitle}</title></head>
<body style=""margin:0;padding:0;background:#f3f6ff;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f3f6ff;padding:24px 12px;"">
    <tr><td align=""center"">
      <table width=""600"" cellspacing=""0"" cellpadding=""0"" style=""width:100%;max-width:600px;background:#fff;border-radius:14px;overflow:hidden;"">
        <tr>
          <td style=""padding:22px 28px;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 52%,#4a2080 100%);text-align:center;"">
            <div style=""color:#fff;font-size:22px;font-weight:700;"">MABA — Admin Alert</div>
          </td>
        </tr>
        <tr>
          <td style=""padding:28px;"">
            <h2 style=""margin:0 0 1rem;color:#1a1a2e;font-size:1.2rem;"">{safeTitle}</h2>
            <p style=""margin:0 0 1.5rem;color:#334155;line-height:1.6;"">{body}</p>
            <a href=""{safeUrl}"" style=""display:inline-block;padding:0.75rem 2rem;background:linear-gradient(135deg,#667eea,#764ba2);color:#fff;border-radius:10px;text-decoration:none;font-weight:600;"">{safeAction}</a>
          </td>
        </tr>
        <tr>
          <td style=""padding:1rem 28px;background:#f8faff;color:#94a3b8;font-size:0.8rem;text-align:center;"">
            MABA Admin Panel — this email was sent to all Admin and Manager accounts.
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
    }
}

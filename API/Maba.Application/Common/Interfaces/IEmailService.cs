using Maba.Application.Common.Emails;

namespace Maba.Application.Common.Interfaces;

/// <summary>
/// Sends request confirmation and optional internal notification emails.
/// Implementations should not throw on failure; log and return instead.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a confirmation email to the customer and optionally a notification to the company inbox.
    /// If customerEmail is null or empty, no customer email is sent (notification may still be sent if configured).
    /// </summary>
    /// <param name="viewRequestUrl">Optional deep link (e.g. account request detail). If null, button uses <c>App:FrontendBaseUrl</c> only.</param>
    Task SendRequestConfirmationAsync(
        string? customerEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email verification message with a link. Implementations should not throw on failure; log and return instead.
    /// </summary>
    Task SendEmailVerificationAsync(string toEmail, string verificationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email message with a reset link.
    /// </summary>
    Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>Shop order confirmation (HTML). No-op when <paramref name="toEmail"/> is empty.</summary>
    Task SendShopOrderConfirmationAsync(
        string? toEmail,
        ShopOrderConfirmationEmailModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Shop order shipped notification (HTML). No-op when <paramref name="toEmail"/> is empty.</summary>
    Task SendShopOrderShippedAsync(
        string? toEmail,
        ShopOrderShippedEmailModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Shop order cancelled (HTML). No-op when <paramref name="toEmail"/> is empty.</summary>
    Task SendShopOrderCancelledAsync(
        string? toEmail,
        ShopOrderCancelledEmailModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Service request cancelled (e.g. 3D, design, laser). No-op when <paramref name="toEmail"/> is empty.</summary>
    Task SendRequestCancelledAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string? viewRequestUrl,
        string? reasonOrNote,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a pre-composed HTML email directly to a single recipient.
    /// Used by AdminNotificationService to fan out alerts to all admin users.
    /// Never throws.
    /// </summary>
    Task SendDirectAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the admin inbox that a customer has opened a new support conversation.
    /// Sends only to <c>NotificationToEmail</c>; never throws.
    /// </summary>
    Task SendSupportChatNotificationAsync(
        string? customerName,
        string? customerEmail,
        string subject,
        string? initialMessage,
        string adminChatUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a workflow status update email for any service request type.
    /// Covers all meaningful transitions: under review, approved, rejected, in progress, completed, etc.
    /// No-op when <paramref name="toEmail"/> is null or empty. Never throws.
    /// </summary>
    /// <param name="newStatus">Normalised status key (e.g. "UnderReview", "Rejected", "Completed").</param>
    /// <param name="rejectionReason">Required when newStatus is "Rejected"; included in email body.</param>
    Task SendRequestStatusUpdateAsync(
        string? toEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        string newStatus,
        string? viewRequestUrl,
        string? rejectionReason = null,
        CancellationToken cancellationToken = default);
}

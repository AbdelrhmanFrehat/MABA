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
    Task SendRequestConfirmationAsync(
        string? customerEmail,
        string? customerName,
        string referenceNumber,
        string requestTypeLabel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email verification message with a link. Implementations should not throw on failure; log and return instead.
    /// </summary>
    Task SendEmailVerificationAsync(string toEmail, string verificationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email message with a reset link.
    /// </summary>
    Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}

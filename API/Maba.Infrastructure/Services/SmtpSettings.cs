namespace Maba.Infrastructure.Services;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    /// <summary>
    /// Optional. If set, a notification email is sent to this address for each request.
    /// </summary>
    public string? NotificationToEmail { get; set; }
}

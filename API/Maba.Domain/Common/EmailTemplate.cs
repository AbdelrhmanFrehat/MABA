namespace Maba.Domain.Common;

public class EmailTemplate : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "WelcomeEmail", "PasswordReset", "OrderConfirmation"
    public string SubjectEn { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public string BodyHtmlEn { get; set; } = string.Empty;
    public string BodyHtmlAr { get; set; } = string.Empty;
    public string? BodyTextEn { get; set; }
    public string? BodyTextAr { get; set; }
    public string? Variables { get; set; } // JSON array of available variables
    public bool IsActive { get; set; } = true;
}


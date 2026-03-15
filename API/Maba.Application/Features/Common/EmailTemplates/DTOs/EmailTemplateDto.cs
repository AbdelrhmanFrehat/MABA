namespace Maba.Application.Features.Common.EmailTemplates.DTOs;

public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public string BodyHtmlEn { get; set; } = string.Empty;
    public string BodyHtmlAr { get; set; } = string.Empty;
    public string? BodyTextEn { get; set; }
    public string? BodyTextAr { get; set; }
    public string? Variables { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


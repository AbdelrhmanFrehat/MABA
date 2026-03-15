namespace Maba.Domain.Common;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; } // e.g., "Email", "Payment", "General"
    public string? DataType { get; set; } // e.g., "String", "Number", "Boolean", "JSON"
    public bool IsPublic { get; set; } = false; // Can be accessed without auth
    public bool IsEncrypted { get; set; } = false; // Should be encrypted in storage
}


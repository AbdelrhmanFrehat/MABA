namespace Maba.Application.Features.Lookups.DTOs;

public class LookupTypeDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
}

public class LookupValueDto
{
    public Guid Id { get; set; }
    public Guid LookupTypeId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
}

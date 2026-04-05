namespace Maba.Application.Features.BusinessInventory.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

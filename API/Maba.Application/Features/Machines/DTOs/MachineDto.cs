namespace Maba.Application.Features.Machines.DTOs;

public class MachineDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public Guid? ImageId { get; set; }
    public Guid? ManualId { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? Location { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public List<MachinePartDto> Parts { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MachineDetailDto : MachineDto
{
    public int PartsCount { get; set; }
    public int ItemsCount { get; set; }
}

public class MachinePartDto
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public string PartNameEn { get; set; } = string.Empty;
    public string PartNameAr { get; set; } = string.Empty;
    public string? PartCode { get; set; }
    public decimal? Price { get; set; }
    public Guid? InventoryId { get; set; }
    public Guid? ImageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ItemMachineLinkDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid MachineId { get; set; }
    public Guid? MachinePartId { get; set; }
    public string MachineNameEn { get; set; } = string.Empty;
    public string? PartNameEn { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


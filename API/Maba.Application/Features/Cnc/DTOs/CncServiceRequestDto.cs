using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.DTOs;

public class CncServiceRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public string ServiceMode { get; set; } = string.Empty;
    
    public Guid? MaterialId { get; set; }
    public string? MaterialNameEn { get; set; }
    public string? MaterialNameAr { get; set; }
    
    public string? PcbMaterial { get; set; }
    public decimal? PcbThickness { get; set; }
    public string? PcbSide { get; set; }
    public string? PcbOperation { get; set; }
    
    public string? OperationType { get; set; }
    
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int Quantity { get; set; }
    
    public string? DepthMode { get; set; }
    public decimal? DepthMm { get; set; }
    
    public string DesignSourceType { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? DesignNotes { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? ProjectDescription { get; set; }
    
    public string? AdminNotes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    
    public CncServiceRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateCncServiceRequestResultDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

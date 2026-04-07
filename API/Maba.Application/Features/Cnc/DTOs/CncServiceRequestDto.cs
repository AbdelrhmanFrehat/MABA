using Maba.Domain.Cnc;
using Maba.Application.Common.ServiceRequests;

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
    public string WorkflowStatus => ServiceRequestWorkflowMapper.FromCnc(Status).ToString();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public static CncServiceRequestDto FromEntity(CncServiceRequest x) => new()
    {
        Id = x.Id,
        ReferenceNumber = x.ReferenceNumber,
        ServiceMode = x.ServiceMode,
        MaterialId = x.MaterialId,
        MaterialNameEn = x.Material?.NameEn,
        MaterialNameAr = x.Material?.NameAr,
        PcbMaterial = x.PcbMaterial,
        PcbThickness = x.PcbThickness,
        PcbSide = x.PcbSide,
        PcbOperation = x.PcbOperation,
        OperationType = x.OperationType,
        WidthMm = x.WidthMm,
        HeightMm = x.HeightMm,
        ThicknessMm = x.ThicknessMm,
        Quantity = x.Quantity,
        DepthMode = x.DepthMode,
        DepthMm = x.DepthMm,
        DesignSourceType = x.DesignSourceType,
        FilePath = x.FilePath,
        FileName = x.FileName,
        DesignNotes = x.DesignNotes,
        CustomerName = x.CustomerName,
        CustomerEmail = x.CustomerEmail,
        CustomerPhone = x.CustomerPhone,
        ProjectDescription = x.ProjectDescription,
        AdminNotes = x.AdminNotes,
        EstimatedPrice = x.EstimatedPrice,
        FinalPrice = x.FinalPrice,
        Status = x.Status,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        ReviewedAt = x.ReviewedAt,
        CompletedAt = x.CompletedAt
    };
}

public class CreateCncServiceRequestResultDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CncServiceRequestsListResult
{
    public List<CncServiceRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

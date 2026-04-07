using Maba.Domain.Laser;
using Maba.Application.Common.ServiceRequests;

namespace Maba.Application.Features.Laser.DTOs;

public class LaserServiceRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public Guid MaterialId { get; set; }
    public string MaterialNameEn { get; set; } = string.Empty;
    public string MaterialNameAr { get; set; } = string.Empty;
    
    public string OperationMode { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;
    
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerNotes { get; set; }
    
    public string? AdminNotes { get; set; }
    public decimal? QuotedPrice { get; set; }
    
    public LaserServiceRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string WorkflowStatus => ServiceRequestWorkflowMapper.FromLaser(Status).ToString();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateLaserServiceRequestResultDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

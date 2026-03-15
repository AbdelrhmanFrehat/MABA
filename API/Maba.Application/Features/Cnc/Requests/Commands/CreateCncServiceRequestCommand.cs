using MediatR;
using Maba.Application.Features.Cnc.DTOs;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class CreateCncServiceRequestCommand : IRequest<CreateCncServiceRequestResultDto>
{
    public string ServiceMode { get; set; } = "routing";
    
    public Guid? MaterialId { get; set; }
    
    public string? PcbMaterial { get; set; }
    public decimal? PcbThickness { get; set; }
    public string? PcbSide { get; set; }
    public string? PcbOperation { get; set; }
    
    public string? OperationType { get; set; }
    
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int Quantity { get; set; } = 1;
    
    public string? DepthMode { get; set; }
    public decimal? DepthMm { get; set; }
    
    public string DesignSourceType { get; set; } = "production";
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? DesignNotes { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? ProjectDescription { get; set; }
}

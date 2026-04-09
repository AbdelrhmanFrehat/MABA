using MediatR;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class CreateLaserServiceRequestCommand : IRequest<CreateLaserServiceRequestResultDto>
{
    public Guid MaterialId { get; set; }
    public string OperationMode { get; set; } = "engrave";
    
    public string ImagePath { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;
    
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerNotes { get; set; }

    /// <summary>Authenticated website user ID, if available.</summary>
    public Guid? UserId { get; set; }
}

using MediatR;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class UpdateLaserServiceRequestCommand : IRequest<LaserServiceRequestDto?>
{
    public Guid Id { get; set; }
    public LaserServiceRequestStatus? Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? QuotedPrice { get; set; }
}

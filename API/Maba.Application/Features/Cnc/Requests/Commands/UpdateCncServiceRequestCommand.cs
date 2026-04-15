using MediatR;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommand : IRequest<CncServiceRequestDto?>
{
    public Guid Id { get; set; }

    public CncServiceRequestStatus? Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
}

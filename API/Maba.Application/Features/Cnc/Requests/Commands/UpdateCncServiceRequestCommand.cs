using MediatR;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class UpdateCncServiceRequestCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    
    public CncServiceRequestStatus? Status { get; set; }
    public string? AdminNotes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
}

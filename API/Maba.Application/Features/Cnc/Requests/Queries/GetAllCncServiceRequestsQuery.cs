using MediatR;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetAllCncServiceRequestsQuery : IRequest<CncServiceRequestsListResult>
{
    public CncServiceRequestStatus? Status { get; set; }
    public string? ServiceMode { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? CreatedFromUtc { get; set; }
    public DateTime? CreatedToUtc { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

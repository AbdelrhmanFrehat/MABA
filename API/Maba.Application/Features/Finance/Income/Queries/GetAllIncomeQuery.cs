using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.Income.Queries;

public class GetAllIncomeQuery : IRequest<List<IncomeDto>>
{
    public Guid? IncomeSourceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}


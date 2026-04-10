using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.Income.Commands;

public class CreateIncomeCommand : IRequest<IncomeDto>
{
    public Guid IncomeSourceId { get; set; }
    public string? RefId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime ReceivedAt { get; set; }
    public Guid EnteredByUserId { get; set; }
}


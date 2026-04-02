using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.Queries;

public class GetFinanceKpisQuery : IRequest<FinanceKpisDto>
{
}

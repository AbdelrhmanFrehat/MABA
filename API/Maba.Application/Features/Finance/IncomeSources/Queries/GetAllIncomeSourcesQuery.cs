using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.IncomeSources.Queries;

public class GetAllIncomeSourcesQuery : IRequest<List<IncomeSourceDto>>
{
}


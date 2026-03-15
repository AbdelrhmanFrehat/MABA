using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Income.Queries;
using Maba.Domain.Finance;
using IncomeEntity = Maba.Domain.Finance.Income;

namespace Maba.Application.Features.Finance.Income.Handlers;

public class GetAllIncomeQueryHandler : IRequestHandler<GetAllIncomeQuery, List<IncomeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllIncomeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IncomeDto>> Handle(GetAllIncomeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<IncomeEntity>()
            .Include(i => i.IncomeSource)
            .Include(i => i.EnteredByUser)
            .AsQueryable();

        if (request.IncomeSourceId.HasValue)
        {
            query = query.Where(i => i.IncomeSourceId == request.IncomeSourceId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(i => i.ReceivedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(i => i.ReceivedAt <= request.ToDate.Value);
        }

        var incomes = await query.ToListAsync(cancellationToken);

        return incomes.Select(i => new IncomeDto
        {
            Id = i.Id,
            IncomeSourceId = i.IncomeSourceId,
            IncomeSourceKey = i.IncomeSource.Key,
            RefId = i.RefId,
            Amount = i.Amount,
            Currency = i.Currency,
            ReceivedAt = i.ReceivedAt,
            EnteredByUserId = i.EnteredByUserId,
            EnteredByUserFullName = i.EnteredByUser.FullName,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        }).ToList();
    }
}


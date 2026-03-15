using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.IncomeSources.Queries;
using Maba.Domain.Finance;

namespace Maba.Application.Features.Finance.IncomeSources.Handlers;

public class GetAllIncomeSourcesQueryHandler : IRequestHandler<GetAllIncomeSourcesQuery, List<IncomeSourceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllIncomeSourcesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IncomeSourceDto>> Handle(GetAllIncomeSourcesQuery request, CancellationToken cancellationToken)
    {
        var sources = await _context.Set<IncomeSource>().ToListAsync(cancellationToken);

        return sources.Select(s => new IncomeSourceDto
        {
            Id = s.Id,
            Key = s.Key,
            NameEn = s.NameEn,
            NameAr = s.NameAr,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}


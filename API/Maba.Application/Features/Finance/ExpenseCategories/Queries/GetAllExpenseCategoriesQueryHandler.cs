using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.ExpenseCategories.Queries;
using Maba.Domain.Finance;

namespace Maba.Application.Features.Finance.ExpenseCategories.Handlers;

public class GetAllExpenseCategoriesQueryHandler : IRequestHandler<GetAllExpenseCategoriesQuery, List<ExpenseCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllExpenseCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpenseCategoryDto>> Handle(GetAllExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Set<ExpenseCategory>().ToListAsync(cancellationToken);

        return categories.Select(c => new ExpenseCategoryDto
        {
            Id = c.Id,
            Key = c.Key,
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}


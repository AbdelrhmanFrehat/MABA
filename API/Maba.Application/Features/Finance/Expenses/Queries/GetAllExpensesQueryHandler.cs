using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Expenses.Queries;
using Maba.Domain.Finance;

namespace Maba.Application.Features.Finance.Expenses.Handlers;

public class GetAllExpensesQueryHandler : IRequestHandler<GetAllExpensesQuery, List<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllExpensesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpenseDto>> Handle(GetAllExpensesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Expense>()
            .Include(e => e.ExpenseCategory)
            .Include(e => e.EnteredByUser)
            .AsQueryable();

        if (request.ExpenseCategoryId.HasValue)
        {
            query = query.Where(e => e.ExpenseCategoryId == request.ExpenseCategoryId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(e => e.SpentAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(e => e.SpentAt <= request.ToDate.Value);
        }

        var expenses = await query.ToListAsync(cancellationToken);

        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            ExpenseCategoryId = e.ExpenseCategoryId,
            ExpenseCategoryKey = e.ExpenseCategory.Key,
            DescriptionEn = e.DescriptionEn,
            DescriptionAr = e.DescriptionAr,
            Amount = e.Amount,
            Currency = e.Currency,
            SpentAt = e.SpentAt,
            ReceiptMediaId = e.ReceiptMediaId,
            EnteredByUserId = e.EnteredByUserId,
            EnteredByUserFullName = e.EnteredByUser.FullName,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        }).ToList();
    }
}


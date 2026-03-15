using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.Expenses.Commands;
using Maba.Application.Features.Finance.DTOs;
using Maba.Domain.Finance;

namespace Maba.Application.Features.Finance.Expenses.Handlers;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Set<ExpenseCategory>()
            .FirstOrDefaultAsync(ec => ec.Id == request.ExpenseCategoryId, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Expense category not found");
        }

        var user = await _context.Set<Domain.Users.User>()
            .FirstOrDefaultAsync(u => u.Id == request.EnteredByUserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (request.ReceiptMediaId.HasValue)
        {
            var mediaExists = await _context.Set<Domain.Media.MediaAsset>()
                .AnyAsync(m => m.Id == request.ReceiptMediaId.Value, cancellationToken);

            if (!mediaExists)
            {
                throw new KeyNotFoundException("Receipt media not found");
            }
        }

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            ExpenseCategoryId = request.ExpenseCategoryId,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            Amount = request.Amount,
            Currency = request.Currency,
            SpentAt = request.SpentAt,
            ReceiptMediaId = request.ReceiptMediaId,
            EnteredByUserId = request.EnteredByUserId
        };

        _context.Set<Expense>().Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExpenseDto
        {
            Id = expense.Id,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            ExpenseCategoryKey = category.Key,
            DescriptionEn = expense.DescriptionEn,
            DescriptionAr = expense.DescriptionAr,
            Amount = expense.Amount,
            Currency = expense.Currency,
            SpentAt = expense.SpentAt,
            ReceiptMediaId = expense.ReceiptMediaId,
            EnteredByUserId = expense.EnteredByUserId,
            EnteredByUserFullName = user.FullName,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }
}


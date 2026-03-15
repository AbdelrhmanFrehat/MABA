using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.Expenses.Queries;

public class GetAllExpensesQuery : IRequest<List<ExpenseDto>>
{
    public Guid? ExpenseCategoryId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}


using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.Expenses.Commands;

public class CreateExpenseCommand : IRequest<ExpenseDto>
{
    public Guid ExpenseCategoryId { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime SpentAt { get; set; }
    public Guid? ReceiptMediaId { get; set; }
    public Guid EnteredByUserId { get; set; }
    public Guid? PaidByUserId { get; set; }
    public Guid? PaymentMethodId { get; set; }
}


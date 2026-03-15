using MediatR;
using Maba.Application.Features.Finance.DTOs;

namespace Maba.Application.Features.Finance.ExpenseCategories.Queries;

public class GetAllExpenseCategoriesQuery : IRequest<List<ExpenseCategoryDto>>
{
}


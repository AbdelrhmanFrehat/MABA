using MediatR;
using Maba.Application.Features.Catalog.Categories.DTOs;

namespace Maba.Application.Features.Catalog.Categories.Queries;

public class GetCategoriesByParentQuery : IRequest<List<CategoryDto>>
{
    public Guid? ParentId { get; set; } // null = root categories
}


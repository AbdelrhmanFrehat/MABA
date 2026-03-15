using MediatR;
using Maba.Application.Features.Catalog.Categories.DTOs;

namespace Maba.Application.Features.Catalog.Categories.Queries;

public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
{
    public bool? IsActive { get; set; }
    public bool IncludeChildren { get; set; } = false;
}


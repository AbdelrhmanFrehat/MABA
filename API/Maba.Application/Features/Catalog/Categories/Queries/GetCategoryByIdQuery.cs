using MediatR;
using Maba.Application.Features.Catalog.Categories.DTOs;

namespace Maba.Application.Features.Catalog.Categories.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
}


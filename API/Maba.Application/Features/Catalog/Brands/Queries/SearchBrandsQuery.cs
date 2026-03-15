using MediatR;
using Maba.Application.Features.Catalog.Brands.DTOs;

namespace Maba.Application.Features.Catalog.Brands.Queries;

public class SearchBrandsQuery : IRequest<List<BrandDto>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}


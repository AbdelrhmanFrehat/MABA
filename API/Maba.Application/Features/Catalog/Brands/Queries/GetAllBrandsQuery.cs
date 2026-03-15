using MediatR;
using Maba.Application.Features.Catalog.Brands.DTOs;

namespace Maba.Application.Features.Catalog.Brands.Queries;

public class GetAllBrandsQuery : IRequest<List<BrandDto>>
{
    public bool? IsActive { get; set; }
}


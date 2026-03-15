using MediatR;
using Maba.Application.Features.Catalog.Brands.DTOs;

namespace Maba.Application.Features.Catalog.Brands.Queries;

public class GetBrandByIdQuery : IRequest<BrandDto>
{
    public Guid Id { get; set; }
}


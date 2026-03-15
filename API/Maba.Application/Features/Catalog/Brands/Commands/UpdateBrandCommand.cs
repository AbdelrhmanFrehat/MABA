using MediatR;
using Maba.Application.Features.Catalog.Brands.DTOs;

namespace Maba.Application.Features.Catalog.Brands.Commands;

public class UpdateBrandCommand : IRequest<BrandDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}


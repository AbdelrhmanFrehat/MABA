using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class UpdateItemCommand : IRequest<ItemDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? GeneralDescriptionEn { get; set; }
    public string? GeneralDescriptionAr { get; set; }
    public Guid ItemStatusId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ILS";
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid> TagIds { get; set; } = new();
}


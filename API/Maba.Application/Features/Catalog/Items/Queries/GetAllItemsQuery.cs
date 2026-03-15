using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetAllItemsQuery : IRequest<List<ItemDto>>
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? ItemStatusId { get; set; }
    public Guid? TagId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}


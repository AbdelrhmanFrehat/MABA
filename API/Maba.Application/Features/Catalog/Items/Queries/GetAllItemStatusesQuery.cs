using MediatR;
using Maba.Application.Features.Catalog.Items.DTOs;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetAllItemStatusesQuery : IRequest<List<ItemStatusDto>>
{
}

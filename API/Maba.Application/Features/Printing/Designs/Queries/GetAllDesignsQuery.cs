using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Designs.Queries;

public class GetAllDesignsQuery : IRequest<List<DesignDto>>
{
    public Guid? UserId { get; set; }
}


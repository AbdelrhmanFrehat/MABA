using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Designs.Queries;

public class GetDesignDetailQuery : IRequest<DesignDetailDto>
{
    public Guid DesignId { get; set; }
}


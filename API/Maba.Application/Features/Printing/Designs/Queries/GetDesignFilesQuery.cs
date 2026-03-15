using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Designs.Queries;

public class GetDesignFilesQuery : IRequest<List<DesignFileDto>>
{
    public Guid DesignId { get; set; }
}


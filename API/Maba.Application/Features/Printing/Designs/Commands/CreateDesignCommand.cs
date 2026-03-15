using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Designs.Commands;

public class CreateDesignCommand : IRequest<DesignDto>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
}


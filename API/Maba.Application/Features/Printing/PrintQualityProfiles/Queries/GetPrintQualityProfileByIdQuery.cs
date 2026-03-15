using MediatR;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Queries;

public class GetPrintQualityProfileByIdQuery : IRequest<PrintQualityProfileDto?>
{
    public Guid Id { get; set; }
}

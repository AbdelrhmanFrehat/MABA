using MediatR;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Queries;

public class GetAllPrintQualityProfilesQuery : IRequest<List<PrintQualityProfileDto>>
{
    public bool ActiveOnly { get; set; } = false;
}

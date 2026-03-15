using MediatR;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Commands;

public class DeletePrintQualityProfileCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

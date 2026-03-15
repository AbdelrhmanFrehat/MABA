using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Printers.Commands;

public class CreatePrinterCommand : IRequest<PrinterDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public decimal BuildVolumeX { get; set; }
    public decimal BuildVolumeY { get; set; }
    public decimal BuildVolumeZ { get; set; }
    public Guid PrintingTechnologyId { get; set; }
}


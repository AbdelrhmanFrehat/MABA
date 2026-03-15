using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Printers.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Printers.Handlers;

public class CreatePrinterCommandHandler : IRequestHandler<CreatePrinterCommand, PrinterDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePrinterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrinterDto> Handle(CreatePrinterCommand request, CancellationToken cancellationToken)
    {
        var technology = await _context.Set<PrintingTechnology>()
            .FirstOrDefaultAsync(t => t.Id == request.PrintingTechnologyId, cancellationToken);

        if (technology == null)
        {
            throw new KeyNotFoundException("Printing technology not found");
        }

        var printer = new Printer
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Vendor = request.Vendor,
            BuildVolumeX = request.BuildVolumeX,
            BuildVolumeY = request.BuildVolumeY,
            BuildVolumeZ = request.BuildVolumeZ,
            PrintingTechnologyId = request.PrintingTechnologyId
        };

        _context.Set<Printer>().Add(printer);
        await _context.SaveChangesAsync(cancellationToken);

        return new PrinterDto
        {
            Id = printer.Id,
            NameEn = printer.NameEn,
            NameAr = printer.NameAr,
            Vendor = printer.Vendor,
            BuildVolumeX = printer.BuildVolumeX,
            BuildVolumeY = printer.BuildVolumeY,
            BuildVolumeZ = printer.BuildVolumeZ,
            PrintingTechnologyId = printer.PrintingTechnologyId,
            PrintingTechnologyKey = technology.Key,
            CreatedAt = printer.CreatedAt,
            UpdatedAt = printer.UpdatedAt
        };
    }
}


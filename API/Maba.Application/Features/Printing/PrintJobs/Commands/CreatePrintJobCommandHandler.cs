using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class CreatePrintJobCommandHandler : IRequestHandler<CreatePrintJobCommand, PrintJobDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePrintJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobDto> Handle(CreatePrintJobCommand request, CancellationToken cancellationToken)
    {
        var slicingJob = await _context.Set<SlicingJob>()
            .FirstOrDefaultAsync(sj => sj.Id == request.SlicingJobId, cancellationToken);

        if (slicingJob == null)
        {
            throw new KeyNotFoundException("Slicing job not found");
        }

        var printer = await _context.Set<Printer>()
            .FirstOrDefaultAsync(p => p.Id == request.PrinterId, cancellationToken);

        if (printer == null)
        {
            throw new KeyNotFoundException("Printer not found");
        }

        var queuedStatus = await _context.Set<PrintJobStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Queued", cancellationToken);

        if (queuedStatus == null)
        {
            throw new KeyNotFoundException("Queued print job status not found");
        }

        var printJob = new PrintJob
        {
            Id = Guid.NewGuid(),
            SlicingJobId = request.SlicingJobId,
            PrinterId = request.PrinterId,
            PrintJobStatusId = queuedStatus.Id
        };

        _context.Set<PrintJob>().Add(printJob);
        await _context.SaveChangesAsync(cancellationToken);

        return new PrintJobDto
        {
            Id = printJob.Id,
            SlicingJobId = printJob.SlicingJobId,
            PrinterId = printJob.PrinterId,
            PrinterNameEn = printer.NameEn,
            PrintJobStatusId = printJob.PrintJobStatusId,
            PrintJobStatusKey = queuedStatus.Key,
            StartedAt = printJob.StartedAt,
            FinishedAt = printJob.FinishedAt,
            ActualMaterialGrams = printJob.ActualMaterialGrams,
            ActualTimeMin = printJob.ActualTimeMin,
            FinalPrice = printJob.FinalPrice,
            CreatedAt = printJob.CreatedAt,
            UpdatedAt = printJob.UpdatedAt
        };
    }
}


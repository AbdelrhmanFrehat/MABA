using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class UpdatePrintJobProgressCommandHandler : IRequestHandler<UpdatePrintJobProgressCommand, PrintJobDto>
{
    private readonly IApplicationDbContext _context;

    public UpdatePrintJobProgressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobDto> Handle(UpdatePrintJobProgressCommand request, CancellationToken cancellationToken)
    {
        var printJob = await _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Include(pj => pj.Printer)
            .FirstOrDefaultAsync(pj => pj.Id == request.PrintJobId, cancellationToken);

        if (printJob == null)
        {
            throw new KeyNotFoundException("Print job not found.");
        }

        printJob.ProgressPercent = Math.Clamp(request.ProgressPercent, 0, 100);
        printJob.UpdatedAt = DateTime.UtcNow;

        if (request.EstimatedCompletionTime.HasValue)
        {
            printJob.EstimatedCompletionTime = request.EstimatedCompletionTime.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PrintJobDto
        {
            Id = printJob.Id,
            SlicingJobId = printJob.SlicingJobId,
            PrinterId = printJob.PrinterId,
            PrinterNameEn = printJob.Printer.NameEn,
            PrintJobStatusId = printJob.PrintJobStatusId,
            PrintJobStatusKey = printJob.PrintJobStatus.Key,
            StartedAt = printJob.StartedAt,
            FinishedAt = printJob.FinishedAt,
            ActualMaterialGrams = printJob.ActualMaterialGrams,
            ActualTimeMin = printJob.ActualTimeMin,
            FinalPrice = printJob.FinalPrice,
            ProgressPercent = printJob.ProgressPercent,
            EstimatedCompletionTime = printJob.EstimatedCompletionTime,
            ErrorMessage = printJob.ErrorMessage,
            CreatedAt = printJob.CreatedAt,
            UpdatedAt = printJob.UpdatedAt
        };
    }
}


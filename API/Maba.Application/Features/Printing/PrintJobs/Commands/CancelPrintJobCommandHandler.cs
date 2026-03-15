using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class CancelPrintJobCommandHandler : IRequestHandler<CancelPrintJobCommand, PrintJobDto>
{
    private readonly IApplicationDbContext _context;

    public CancelPrintJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobDto> Handle(CancelPrintJobCommand request, CancellationToken cancellationToken)
    {
        var printJob = await _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Include(pj => pj.Printer)
            .Include(pj => pj.SlicingJob)
            .ThenInclude(sj => sj.SlicingProfile)
            .ThenInclude(sp => sp.Material)
            .FirstOrDefaultAsync(pj => pj.Id == request.PrintJobId, cancellationToken);

        if (printJob == null)
        {
            throw new KeyNotFoundException("Print job not found.");
        }

        var cancelledStatus = await _context.Set<PrintJobStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Cancelled", cancellationToken);

        if (cancelledStatus == null)
        {
            throw new InvalidOperationException("Cancelled status not found.");
        }

        // Only allow cancellation if job is not already completed or cancelled
        if (printJob.PrintJobStatus.Key == "Completed" || printJob.PrintJobStatus.Key == "Cancelled")
        {
            throw new InvalidOperationException($"Cannot cancel print job with status: {printJob.PrintJobStatus.Key}");
        }

        printJob.PrintJobStatusId = cancelledStatus.Id;
        printJob.ErrorMessage = request.Reason ?? "Cancelled by user";
        printJob.FinishedAt = DateTime.UtcNow;
        printJob.UpdatedAt = DateTime.UtcNow;

        // Update printer status to Idle
        printJob.Printer.CurrentStatus = "Idle";

        // CRITICAL LOGIC: Restore material inventory if material was deducted
        if (printJob.ActualMaterialGrams.HasValue && printJob.SlicingJob.SlicingProfile.MaterialId != Guid.Empty)
        {
            // Find inventory for the material (if it exists as an item)
            // This is a simplified approach - in production, you'd have a MaterialInventory table
            // For now, we'll just log that material should be restored
            // TODO: Implement material inventory restoration logic
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


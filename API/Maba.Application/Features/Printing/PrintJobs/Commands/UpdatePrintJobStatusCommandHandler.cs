using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class UpdatePrintJobStatusCommandHandler : IRequestHandler<UpdatePrintJobStatusCommand, PrintJobDto>
{
    private readonly IApplicationDbContext _context;

    public UpdatePrintJobStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobDto> Handle(UpdatePrintJobStatusCommand request, CancellationToken cancellationToken)
    {
        var printJob = await _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Include(pj => pj.Printer)
            .FirstOrDefaultAsync(pj => pj.Id == request.PrintJobId, cancellationToken);

        if (printJob == null)
        {
            throw new KeyNotFoundException("Print job not found.");
        }

        printJob.PrintJobStatusId = request.PrintJobStatusId;
        printJob.UpdatedAt = DateTime.UtcNow;

        if (request.ProgressPercent.HasValue)
        {
            printJob.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
        }

        if (request.EstimatedCompletionTime.HasValue)
        {
            printJob.EstimatedCompletionTime = request.EstimatedCompletionTime.Value;
        }

        if (request.ErrorMessage != null)
        {
            printJob.ErrorMessage = request.ErrorMessage;
        }

        // Update printer status based on print job status
        var status = await _context.Set<PrintJobStatus>()
            .FirstOrDefaultAsync(s => s.Id == request.PrintJobStatusId, cancellationToken);

        if (status != null)
        {
            printJob.Printer.CurrentStatus = status.Key switch
            {
                "Printing" => "Printing",
                "Paused" => "Paused",
                "Completed" or "Cancelled" or "Failed" => "Idle",
                _ => printJob.Printer.CurrentStatus
            };
        }

        // Set StartedAt when status changes to Printing
        if (status?.Key == "Printing" && printJob.StartedAt == null)
        {
            printJob.StartedAt = DateTime.UtcNow;
        }

        // Set FinishedAt when status changes to Completed, Cancelled, or Failed
        if ((status?.Key == "Completed" || status?.Key == "Cancelled" || status?.Key == "Failed") && printJob.FinishedAt == null)
        {
            printJob.FinishedAt = DateTime.UtcNow;
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


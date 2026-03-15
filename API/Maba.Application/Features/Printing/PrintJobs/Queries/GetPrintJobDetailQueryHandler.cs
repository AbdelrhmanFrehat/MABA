using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class GetPrintJobDetailQueryHandler : IRequestHandler<GetPrintJobDetailQuery, PrintJobDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPrintJobDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobDetailDto> Handle(GetPrintJobDetailQuery request, CancellationToken cancellationToken)
    {
        var printJob = await _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Include(pj => pj.Printer)
            .ThenInclude(p => p.PrintingTechnology)
            .Include(pj => pj.SlicingJob)
            .ThenInclude(sj => sj.SlicingJobStatus)
            .Include(pj => pj.SlicingJob)
            .ThenInclude(sj => sj.DesignFile)
            .ThenInclude(df => df.MediaAsset)
            .FirstOrDefaultAsync(pj => pj.Id == request.PrintJobId, cancellationToken);

        if (printJob == null)
        {
            throw new KeyNotFoundException("Print job not found.");
        }

        return new PrintJobDetailDto
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
            UpdatedAt = printJob.UpdatedAt,
            SlicingJob = new SlicingJobDto
            {
                Id = printJob.SlicingJob.Id,
                DesignFileId = printJob.SlicingJob.DesignFileId,
                SlicingProfileId = printJob.SlicingJob.SlicingProfileId,
                SlicingJobStatusId = printJob.SlicingJob.SlicingJobStatusId,
                SlicingJobStatusKey = printJob.SlicingJob.SlicingJobStatus.Key,
                EstimatedTimeMin = printJob.SlicingJob.EstimatedTimeMin,
                EstimatedMaterialGrams = printJob.SlicingJob.EstimatedMaterialGrams,
                PriceEstimate = printJob.SlicingJob.PriceEstimate,
                EstimatedCost = printJob.SlicingJob.EstimatedCost,
                ErrorMessage = printJob.SlicingJob.ErrorMessage,
                OutputFileUrl = printJob.SlicingJob.OutputFileUrl,
                CompletedAt = printJob.SlicingJob.CompletedAt,
                CreatedAt = printJob.SlicingJob.CreatedAt,
                UpdatedAt = printJob.SlicingJob.UpdatedAt
            },
            Printer = new PrinterDto
            {
                Id = printJob.Printer.Id,
                NameEn = printJob.Printer.NameEn,
                NameAr = printJob.Printer.NameAr,
                Vendor = printJob.Printer.Vendor,
                BuildVolumeX = printJob.Printer.BuildVolumeX,
                BuildVolumeY = printJob.Printer.BuildVolumeY,
                BuildVolumeZ = printJob.Printer.BuildVolumeZ,
                PrintingTechnologyId = printJob.Printer.PrintingTechnologyId,
                PrintingTechnologyKey = printJob.Printer.PrintingTechnology.Key,
                IsActive = printJob.Printer.IsActive,
                CurrentStatus = printJob.Printer.CurrentStatus,
                Location = printJob.Printer.Location,
                CreatedAt = printJob.Printer.CreatedAt,
                UpdatedAt = printJob.Printer.UpdatedAt
            }
        };
    }
}


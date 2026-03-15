using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintJobs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class GetActivePrintJobsQueryHandler : IRequestHandler<GetActivePrintJobsQuery, List<PrintJobDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActivePrintJobsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrintJobDto>> Handle(GetActivePrintJobsQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = await _context.Set<PrintJobStatus>()
            .Where(s => s.Key == "Queued" || s.Key == "Printing" || s.Key == "Paused")
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var query = _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Include(pj => pj.Printer)
            .Where(pj => activeStatuses.Contains(pj.PrintJobStatusId));

        if (request.PrinterId.HasValue)
        {
            query = query.Where(pj => pj.PrinterId == request.PrinterId.Value);
        }

        var printJobs = await query
            .OrderByDescending(pj => pj.CreatedAt)
            .ToListAsync(cancellationToken);

        return printJobs.Select(pj => new PrintJobDto
        {
            Id = pj.Id,
            SlicingJobId = pj.SlicingJobId,
            PrinterId = pj.PrinterId,
            PrinterNameEn = pj.Printer.NameEn,
            PrintJobStatusId = pj.PrintJobStatusId,
            PrintJobStatusKey = pj.PrintJobStatus.Key,
            StartedAt = pj.StartedAt,
            FinishedAt = pj.FinishedAt,
            ActualMaterialGrams = pj.ActualMaterialGrams,
            ActualTimeMin = pj.ActualTimeMin,
            FinalPrice = pj.FinalPrice,
            ProgressPercent = pj.ProgressPercent,
            EstimatedCompletionTime = pj.EstimatedCompletionTime,
            ErrorMessage = pj.ErrorMessage,
            CreatedAt = pj.CreatedAt,
            UpdatedAt = pj.UpdatedAt
        }).ToList();
    }
}


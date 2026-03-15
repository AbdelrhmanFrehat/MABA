using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.PrintJobs.Queries;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintJobs.Handlers;

public class GetAllPrintJobsQueryHandler : IRequestHandler<GetAllPrintJobsQuery, List<PrintJobDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPrintJobsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrintJobDto>> Handle(GetAllPrintJobsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PrintJob>()
            .Include(pj => pj.Printer)
            .Include(pj => pj.PrintJobStatus)
            .AsQueryable();

        if (request.PrinterId.HasValue)
        {
            query = query.Where(pj => pj.PrinterId == request.PrinterId.Value);
        }

        if (request.PrintJobStatusId.HasValue)
        {
            query = query.Where(pj => pj.PrintJobStatusId == request.PrintJobStatusId.Value);
        }

        var jobs = await query.ToListAsync(cancellationToken);

        return jobs.Select(j => new PrintJobDto
        {
            Id = j.Id,
            SlicingJobId = j.SlicingJobId,
            PrinterId = j.PrinterId,
            PrinterNameEn = j.Printer.NameEn,
            PrintJobStatusId = j.PrintJobStatusId,
            PrintJobStatusKey = j.PrintJobStatus.Key,
            StartedAt = j.StartedAt,
            FinishedAt = j.FinishedAt,
            ActualMaterialGrams = j.ActualMaterialGrams,
            ActualTimeMin = j.ActualTimeMin,
            FinalPrice = j.FinalPrice,
            CreatedAt = j.CreatedAt,
            UpdatedAt = j.UpdatedAt
        }).ToList();
    }
}


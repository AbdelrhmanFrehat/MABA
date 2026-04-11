using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/fiscal-periods")]
[Authorize]
public class FiscalPeriodsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public FiscalPeriodsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetFiscalPeriods(
        [FromQuery] bool? isClosed,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<FiscalPeriod>()
            .Include(x => x.FiscalYear)
            .AsQueryable();

        if (isClosed.HasValue)
            query = query.Where(x => x.IsClosed == isClosed.Value);

        var periods = await query
            .OrderBy(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.StartDate,
                x.EndDate,
                x.PeriodNumber,
                x.IsClosed,
                FiscalYearId   = x.FiscalYearId,
                FiscalYearName = x.FiscalYear.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(periods);
    }

    [HttpGet("current")]
    public async Task<ActionResult> GetCurrentPeriod(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var period = await _context.Set<FiscalPeriod>()
            .Include(x => x.FiscalYear)
            .Where(x => !x.IsClosed && x.StartDate <= now && now <= x.EndDate)
            .OrderBy(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (period == null) return NotFound(new { error = "No open fiscal period found for today." });

        return Ok(new
        {
            period.Id,
            period.Name,
            period.StartDate,
            period.EndDate,
            period.PeriodNumber,
            period.IsClosed,
            FiscalYearId   = period.FiscalYearId,
            FiscalYearName = period.FiscalYear.Name
        });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Maba.Domain.Printing;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Workers;

public class PrintJobWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrintJobWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly Dictionary<Guid, DateTime> _activePrintJobs = new();

    public PrintJobWorker(IServiceProvider serviceProvider, ILogger<PrintJobWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PrintJobWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingPrintJobs(stoppingToken);
                await UpdateActivePrintJobs(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when shutdown is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing print jobs");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }
    }

    private async Task ProcessPendingPrintJobs(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Check if database is ready (migrations applied)
            if (!await context.Database.CanConnectAsync(cancellationToken))
            {
                return;
            }

            var queuedStatus = await context.PrintJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Queued", cancellationToken);

            if (queuedStatus == null) return;

            var printingStatus = await context.PrintJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Printing", cancellationToken);

            var queuedJobs = await context.PrintJobs
                .Include(pj => pj.SlicingJob)
                .ThenInclude(sj => sj!.SlicingProfile)
                .Include(pj => pj.Printer)
                .Where(pj => pj.PrintJobStatusId == queuedStatus.Id)
                .OrderBy(pj => pj.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

        foreach (var job in queuedJobs)
        {
            try
            {
                _logger.LogInformation("Starting print job {JobId}", job.Id);

                if (printingStatus != null)
                {
                    job.PrintJobStatusId = printingStatus.Id;
                }
                job.StartedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                // Track active job
                if (job.SlicingJob?.EstimatedTimeMin != null)
                {
                    var estimatedCompletion = DateTime.UtcNow.AddMinutes(job.SlicingJob.EstimatedTimeMin.Value);
                    _activePrintJobs[job.Id] = estimatedCompletion;
                }

                _logger.LogInformation("Print job {JobId} started", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job {JobId}", job.Id);
            }
        }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 207 || sqlEx.Number == 208)
        {
            // Invalid column name or Invalid object name - database schema not ready yet
            // This is expected before migrations are applied, so just log and continue
            _logger.LogDebug("Database schema not ready yet for print jobs: {Message}", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPendingPrintJobs");
        }
    }

    private async Task UpdateActivePrintJobs(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Check if database is ready (migrations applied)
            if (!await context.Database.CanConnectAsync(cancellationToken))
            {
                return;
            }

            var printingStatus = await context.PrintJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Printing", cancellationToken);

            if (printingStatus == null) return;

            var completedStatus = await context.PrintJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Completed", cancellationToken);

            var activeJobs = await context.PrintJobs
                .Include(pj => pj.SlicingJob)
                .ThenInclude(sj => sj!.SlicingProfile)
                .Where(pj => pj.PrintJobStatusId == printingStatus.Id)
                .ToListAsync(cancellationToken);

        foreach (var job in activeJobs)
        {
            try
            {
                // Check if job should be completed
                if (_activePrintJobs.TryGetValue(job.Id, out var estimatedCompletion))
                {
                    if (DateTime.UtcNow >= estimatedCompletion)
                    {
                        // Complete the job
                        if (completedStatus != null)
                        {
                            job.PrintJobStatusId = completedStatus.Id;
                        }
                        job.FinishedAt = DateTime.UtcNow;

                        // Calculate actual values
                        if (job.StartedAt.HasValue)
                        {
                            var duration = job.FinishedAt.Value - job.StartedAt.Value;
                            job.ActualTimeMin = (int)duration.TotalMinutes;
                        }

                        if (job.SlicingJob?.EstimatedMaterialGrams != null)
                        {
                            var random = new Random();
                            var variance = (decimal)(random.NextDouble() * 0.2 - 0.1); // ±10% variance
                            job.ActualMaterialGrams = job.SlicingJob.EstimatedMaterialGrams.Value * (1 + variance);
                        }

                        if (job.SlicingJob?.PriceEstimate != null)
                        {
                            job.FinalPrice = job.SlicingJob.PriceEstimate.Value;
                        }

                        _activePrintJobs.Remove(job.Id);

                        await context.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Print job {JobId} completed", job.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating print job {JobId}", job.Id);
            }
        }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 207 || sqlEx.Number == 208)
        {
            // Invalid column name or Invalid object name - database schema not ready yet
            // This is expected before migrations are applied, so just log and continue
            _logger.LogDebug("Database schema not ready yet for active print jobs: {Message}", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateActivePrintJobs");
        }
    }
}


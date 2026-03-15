using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Maba.Domain.Printing;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Workers;

public class SlicingJobWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SlicingJobWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public SlicingJobWorker(IServiceProvider serviceProvider, ILogger<SlicingJobWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SlicingJobWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingSlicingJobs(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when shutdown is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing slicing jobs");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }
    }

    private async Task ProcessPendingSlicingJobs(CancellationToken cancellationToken)
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

            // Get pending slicing jobs
            var pendingStatus = await context.SlicingJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Pending", cancellationToken);

            if (pendingStatus == null) return;

            var processingStatus = await context.SlicingJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Processing", cancellationToken);

            var completedStatus = await context.SlicingJobStatuses
                .FirstOrDefaultAsync(s => s.Key == "Completed", cancellationToken);

            var pendingJobs = await context.SlicingJobs
                .Include(sj => sj.SlicingProfile)
                .Include(sj => sj.DesignFile)
                .Where(sj => sj.SlicingJobStatusId == pendingStatus.Id)
                .OrderBy(sj => sj.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

        foreach (var job in pendingJobs)
        {
            try
            {
                _logger.LogInformation("Processing slicing job {JobId}", job.Id);

                // Update to processing
                if (processingStatus != null)
                {
                    job.SlicingJobStatusId = processingStatus.Id;
                }

                await context.SaveChangesAsync(cancellationToken);

                // Simulate slicing process
                await SimulateSlicing(job, cancellationToken);

                // Update job with results
                if (completedStatus != null)
                {
                    job.SlicingJobStatusId = completedStatus.Id;
                }
                job.CompletedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Slicing job {JobId} completed", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing slicing job {JobId}", job.Id);
            }
        }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 207 || sqlEx.Number == 208)
        {
            // Invalid column name or Invalid object name - database schema not ready yet
            // This is expected before migrations are applied, so just log and continue
            _logger.LogDebug("Database schema not ready yet for slicing jobs: {Message}", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPendingSlicingJobs");
        }
    }

    private async Task SimulateSlicing(SlicingJob job, CancellationToken cancellationToken)
    {
        // Simulate slicing time
        await Task.Delay(2000, cancellationToken);

        // Calculate estimated values based on profile
        var random = new Random();
        job.EstimatedTimeMin = random.Next(30, 180); // 30-180 minutes
        job.EstimatedMaterialGrams = (decimal)(random.NextDouble() * 100 + 50); // 50-150 grams

        // Calculate price estimate (material cost + time cost)
        if (job.SlicingProfile?.Material != null)
        {
            var materialCost = job.EstimatedMaterialGrams.Value * job.SlicingProfile.Material.PricePerGram;
            var timeCost = (job.EstimatedTimeMin.Value / 60m) * 10m; // $10 per hour
            job.PriceEstimate = materialCost + timeCost;
        }
    }
}


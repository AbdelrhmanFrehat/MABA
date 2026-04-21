using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IJobsService
{
    Task<IReadOnlyList<ControlCenterJobListItem>> GetJobsAsync(string? status = null, string? machineType = null, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> GetJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> RunActionAsync(Guid id, string action, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> MarkReadyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> StartJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> CompleteJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> FailJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ControlCenterJobDetail?> CancelJobAsync(Guid id, CancellationToken cancellationToken = default);
}

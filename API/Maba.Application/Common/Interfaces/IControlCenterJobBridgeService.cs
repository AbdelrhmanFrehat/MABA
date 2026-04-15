using Maba.Application.Common.ControlCenterJobs;

namespace Maba.Application.Common.Interfaces;

public interface IControlCenterJobBridgeService
{
    Task EnsureJobAsync(ControlCenterJobBridgeDefinition definition, CancellationToken cancellationToken = default);
}

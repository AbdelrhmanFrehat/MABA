using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class ActiveProductionJobService : IActiveProductionJobService
{
    private ActiveProductionJobContext? _currentJob;

    public ActiveProductionJobContext? CurrentJob => _currentJob;

    public event EventHandler? CurrentJobChanged;

    public void SetCurrentJob(ActiveProductionJobContext? job)
    {
        _currentJob = job;
        CurrentJobChanged?.Invoke(this, EventArgs.Empty);
    }
}

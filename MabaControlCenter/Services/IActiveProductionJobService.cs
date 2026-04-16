using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IActiveProductionJobService
{
    ActiveProductionJobContext? CurrentJob { get; }
    event EventHandler? CurrentJobChanged;
    void SetCurrentJob(ActiveProductionJobContext? job);
}

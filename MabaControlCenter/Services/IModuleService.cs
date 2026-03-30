using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IModuleService
{
    ObservableCollection<ModuleInfo> AvailableModules { get; }
}

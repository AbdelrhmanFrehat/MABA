using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class ModuleService : IModuleService
{
    public ObservableCollection<ModuleInfo> AvailableModules { get; } = new();

    public ModuleService()
    {
        AvailableModules.Add(new ModuleInfo
        {
            Name = "Dexter MacroPad",
            Code = "DEXTER-CAL",
            Description = "MacroPad key mapping (a–p) for MABA Dexter VP1",
            Version = "1.0.0",
            IsInstalled = true,
            IsEnabled = true,
            SupportedProductCodes = new List<string> { "DEXTER-VP1" }
        });
        AvailableModules.Add(new ModuleInfo
        {
            Name = "SCARA Control",
            Code = "SCARA-CTRL",
            Description = "Control panel for MABA SCARA robots",
            Version = "1.0.0",
            IsInstalled = false,
            IsEnabled = false,
            SupportedProductCodes = new List<string> { "MABA-SCARA" }
        });
        AvailableModules.Add(new ModuleInfo
        {
            Name = "CNC Manager",
            Code = "CNC-MGR",
            Description = "Control and monitoring tools for MABA CNC systems",
            Version = "1.0.0",
            IsInstalled = true,
            IsEnabled = true,
            SupportedProductCodes = new List<string> { "MABA-CNC" }
        });
    }
}

namespace MabaControlCenter.Models;

public class ModuleInfo
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public bool IsInstalled { get; set; }
    public bool IsEnabled { get; set; }
    public List<string> SupportedProductCodes { get; set; } = new();
}

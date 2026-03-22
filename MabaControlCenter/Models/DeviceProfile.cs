namespace MabaControlCenter.Models;

public class DeviceProfile
{
    public string ProductName { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string FirmwareVersion { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string ConnectionType { get; set; } = "";
    public bool IsSupported { get; set; }
    public string RecommendedModule { get; set; } = "";

    public string SupportStatusDisplay => IsSupported ? "Supported" : "Not supported";
}

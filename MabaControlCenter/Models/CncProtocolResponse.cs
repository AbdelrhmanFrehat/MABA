namespace MabaControlCenter.Models;

public class CncProtocolResponse
{
    public string RawText { get; init; } = string.Empty;
    public CncProtocolMessageType MessageType { get; init; }
    public string? Message { get; init; }
    public CncDeviceState? DeviceState { get; init; }
    public decimal? X { get; init; }
    public decimal? Y { get; init; }
    public decimal? Z { get; init; }

    public bool IsError => MessageType == CncProtocolMessageType.Error || MessageType == CncProtocolMessageType.LimitHit;
}

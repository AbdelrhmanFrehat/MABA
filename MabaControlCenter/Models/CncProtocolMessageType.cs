namespace MabaControlCenter.Models;

public enum CncProtocolMessageType
{
    Unknown,
    Acknowledgement,
    Error,
    Ready,
    HomeDone,
    Stopped,
    LimitHit,
    Status,
    Position
}

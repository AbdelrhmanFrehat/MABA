namespace MabaControlCenter.Models;

public class CncProtocolCommandSpec
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Commands { get; init; }
    public required IReadOnlyList<CncProtocolMessageType> TerminalMessageTypes { get; init; }
    public int TimeoutMs { get; init; } = 1200;
    public bool AllowLegacyStatusWords { get; init; } = true;
}

using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncProtocolCommandFormatter
{
    public CncProtocolCommandSpec CreateEnableCommand() => new()
    {
        Name = "Enable Motors",
        Commands = new[] { "ENABLE" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status, CncProtocolMessageType.Ready }
    };

    public CncProtocolCommandSpec CreateDisableCommand() => new()
    {
        Name = "Disable Motors",
        Commands = new[] { "DISABLE" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status }
    };

    public CncProtocolCommandSpec CreateStatusCommand() => new()
    {
        Name = "Status",
        Commands = new[] { "STATUS" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Status, CncProtocolMessageType.Ready, CncProtocolMessageType.Position },
        TimeoutMs = 1000
    };

    public CncProtocolCommandSpec CreateHomeCommand() => new()
    {
        Name = "Home",
        Commands = new[] { "HOME", "H" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.HomeDone, CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status },
        TimeoutMs = 2500
    };

    public CncProtocolCommandSpec CreateStopCommand() => new()
    {
        Name = "Stop",
        Commands = new[] { "STOP" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Stopped, CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status }
    };

    public CncProtocolCommandSpec CreateJogCommand(string axis, int steps, bool positive) => new()
    {
        Name = $"Jog {axis.ToUpperInvariant()}",
        Commands = new[] { $"{(positive ? "+" : "-")}{steps}{axis.ToLowerInvariant()}" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Position, CncProtocolMessageType.Status },
        TimeoutMs = 1500
    };

    public CncProtocolCommandSpec CreateResetCommand() => new()
    {
        Name = "Reset",
        Commands = new[] { "RESET" },
        TerminalMessageTypes = new[] { CncProtocolMessageType.Acknowledgement, CncProtocolMessageType.Status, CncProtocolMessageType.Ready }
    };
}

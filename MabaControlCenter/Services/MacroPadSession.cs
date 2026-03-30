using System.IO.Ports;
using System.Text;

namespace MabaControlCenter.Services;

/// <summary>Serial session at 115200 matching Python MacroPad device protocol, plus full simulation.</summary>
public sealed class MacroPadSession : IDisposable
{
    private readonly ILoggingService _log;
    private SerialPort? _port;
    private readonly bool _simulate;
    private readonly Dictionary<int, (int mods, List<string> keys)> _simState = new();

    public MacroPadSession(ILoggingService log, bool simulate)
    {
        _log = log;
        _simulate = simulate;
        for (var i = 0; i < MacroPadProtocol.BtnCount; i++)
            _simState[i] = (0, new List<string> { ((char)('a' + i)).ToString() });
    }

    public bool IsOpen => _simulate || (_port?.IsOpen ?? false);

    public bool Open(string portName)
    {
        Close();
        if (_simulate)
        {
            _log.AddLog("MacroPad", $"[sim] open {portName}", "Info");
            return true;
        }
        try
        {
            _port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One) { NewLine = "\n", Encoding = Encoding.UTF8 };
            _port.Open();
            _port.DiscardInBuffer();
            WriteLineRaw("PING");
            var pong = ReadLineRaw(600);
            if (!string.Equals(pong?.Trim(), "PONG", StringComparison.OrdinalIgnoreCase))
            {
                _port.Close();
                _port.Dispose();
                _port = null;
                return false;
            }
            WriteLineRaw("GETINFO");
            _ = ReadUntil("END", 1500);
            return true;
        }
        catch
        {
            try { _port?.Close(); _port?.Dispose(); } catch { /* ignore */ }
            _port = null;
            return false;
        }
    }

    public void Close()
    {
        if (_port != null)
        {
            try { if (_port.IsOpen) _port.Close(); _port.Dispose(); } catch { /* ignore */ }
            _port = null;
        }
    }

    public void Dispose() => Close();

    public List<string> GetFromDevice()
    {
        if (_simulate)
        {
            var lines = new List<string>();
            for (var i = 0; i < MacroPadProtocol.BtnCount; i++)
            {
                var (m, k) = _simState[i];
                lines.Add($"MAP {i} {m} {string.Join(",", k)}");
            }
            lines.Add("END");
            return lines;
        }
        WriteLineRaw("GET");
        return ReadUntil("END", 1500);
    }

    public void SetButton(int index, int mods, IReadOnlyList<string> keys)
    {
        var blob = string.Join(",", keys);
        var cmd = $"SET {index} {mods} {blob}";
        if (_simulate)
        {
            _simState[index] = (mods, keys.ToList());
            _log.AddLog("MacroPad", $"[sim] {cmd} -> OK", "Info");
            return;
        }
        WriteLineRaw(cmd);
        var resp = ReadLineRaw(2000)?.Trim();
        if (!string.Equals(resp, "OK", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"SET failed at {index}: {resp}");
    }

    public void SaveToDevice()
    {
        if (_simulate)
        {
            _log.AddLog("MacroPad", "[sim] SAVE -> OK", "Info");
            return;
        }
        WriteLineRaw("SAVE");
        var r = ReadLineRaw(2000)?.Trim();
        if (!string.Equals(r, "OK", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"SAVE failed: {r}");
    }

    public void DefaultsOnDevice()
    {
        if (_simulate)
        {
            for (var i = 0; i < MacroPadProtocol.BtnCount; i++)
                _simState[i] = (0, new List<string> { ((char)('A' + i)).ToString() });
            _log.AddLog("MacroPad", "[sim] DEFAULTS -> OK", "Info");
            return;
        }
        WriteLineRaw("DEFAULTS");
        var r = ReadLineRaw(2000)?.Trim();
        if (!string.Equals(r, "OK", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"DEFAULTS failed: {r}");
    }

    public void SendProbeKeys(string[] probeKeys)
    {
        for (var i = 0; i < probeKeys.Length && i < MacroPadProtocol.BtnCount; i++)
        {
            SetButton(i, 0, new[] { probeKeys[i] });
        }
    }

    private void WriteLineRaw(string s)
    {
        if (_port == null || !_port.IsOpen) return;
        _port.DiscardInBuffer();
        _port.Write(s + "\n");
        _port.BaseStream.Flush();
    }

    private string? ReadLineRaw(int timeoutMs)
    {
        if (_port == null || !_port.IsOpen) return null;
        _port.ReadTimeout = timeoutMs;
        try
        {
            return _port.ReadLine()?.Trim();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    private List<string> ReadUntil(string endToken, int timeoutMs)
    {
        var lines = new List<string>();
        if (_port == null || !_port.IsOpen) return lines;
        _port.ReadTimeout = timeoutMs;
        while (true)
        {
            string? line;
            try { line = _port.ReadLine()?.Trim(); }
            catch (TimeoutException) { break; }
            if (string.IsNullOrEmpty(line)) break;
            lines.Add(line);
            if (string.Equals(line, endToken, StringComparison.OrdinalIgnoreCase))
                break;
        }
        return lines;
    }
}

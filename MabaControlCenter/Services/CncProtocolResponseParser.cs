using System.Globalization;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncProtocolResponseParser
{
    public CncProtocolResponse Parse(string rawText)
    {
        var normalized = (rawText ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return new CncProtocolResponse
            {
                RawText = rawText ?? string.Empty,
                MessageType = CncProtocolMessageType.Unknown,
                Message = "Empty response."
            };
        }

        if (normalized.Equals("OK", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Acknowledgement,
                Message = "OK"
            };
        }

        if (normalized.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Error,
                Message = normalized["ERR:".Length..].Trim()
            };
        }

        if (normalized.Equals("HOME DONE", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.HomeDone,
                Message = normalized,
                DeviceState = CncDeviceState.Idle
            };
        }

        if (normalized.Equals("STOPPED", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Stopped,
                Message = normalized,
                DeviceState = CncDeviceState.Stopped
            };
        }

        if (normalized.Equals("LIMIT HIT", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.LimitHit,
                Message = normalized,
                DeviceState = CncDeviceState.LimitHit
            };
        }

        if (normalized.Equals("READY", StringComparison.OrdinalIgnoreCase))
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Ready,
                Message = normalized,
                DeviceState = CncDeviceState.Ready
            };
        }

        if (normalized.StartsWith("STATUS:", StringComparison.OrdinalIgnoreCase))
        {
            var stateText = normalized["STATUS:".Length..].Trim();
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Status,
                Message = stateText,
                DeviceState = ParseState(stateText)
            };
        }

        if (normalized.StartsWith("POS:", StringComparison.OrdinalIgnoreCase))
        {
            var payload = normalized["POS:".Length..].Trim();
            var parts = payload.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 &&
                decimal.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                decimal.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                decimal.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                return new CncProtocolResponse
                {
                    RawText = normalized,
                    MessageType = CncProtocolMessageType.Position,
                    X = x,
                    Y = y,
                    Z = z,
                    Message = payload
                };
            }
        }

        var legacyState = ParseState(normalized);
        if (legacyState != CncDeviceState.Unknown)
        {
            return new CncProtocolResponse
            {
                RawText = normalized,
                MessageType = CncProtocolMessageType.Status,
                Message = normalized,
                DeviceState = legacyState
            };
        }

        return new CncProtocolResponse
        {
            RawText = normalized,
            MessageType = CncProtocolMessageType.Unknown,
            Message = normalized
        };
    }

    private static CncDeviceState ParseState(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "READY" => CncDeviceState.Ready,
            "IDLE" => CncDeviceState.Idle,
            "RUNNING" => CncDeviceState.Running,
            "HOMING" => CncDeviceState.Homing,
            "PAUSED" => CncDeviceState.Paused,
            "STOPPED" => CncDeviceState.Stopped,
            "ALARM" => CncDeviceState.Alarm,
            "ERROR" => CncDeviceState.Error,
            "LIMIT" => CncDeviceState.LimitHit,
            "LIMIT HIT" => CncDeviceState.LimitHit,
            "DISCONNECTED" => CncDeviceState.Disconnected,
            _ => CncDeviceState.Unknown
        };
    }
}

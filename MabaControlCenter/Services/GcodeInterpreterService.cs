using System.Globalization;
using System.Text.RegularExpressions;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class GcodeInterpreterService
{
    private static readonly Regex TokenRegex = new(@"([A-Za-z])([+-]?(?:\d+\.?\d*|\.\d+))", RegexOptions.Compiled);
    private static readonly Regex InlineCommentRegex = new(@"\(.*?\)", RegexOptions.Compiled);

    public GcodeInterpreterResult Interpret(IEnumerable<string> lines)
    {
        var result = new GcodeInterpreterResult();
        var state = new GcodeModalState();
        var sourceLines = lines.ToList();

        for (var index = 0; index < sourceLines.Count; index++)
        {
            var rawLine = sourceLines[index];
            var lineNumber = index + 1;
            var commentText = ExtractComment(rawLine);
            var sanitized = StripComments(rawLine).Trim();

            var interpreted = new GcodeInterpretedCommand
            {
                SourceLineNumber = lineNumber,
                RawLine = rawLine,
                SanitizedLine = sanitized,
                CommentText = commentText,
                Units = state.Units,
                DistanceMode = state.DistanceMode,
                MotionMode = state.MotionMode,
                Plane = state.Plane,
                SpindleState = state.SpindleState,
                SpindleSpeed = state.SpindleSpeed,
                ToolNumber = state.ToolNumber,
                StartX = state.Coordinates.WorkX,
                StartY = state.Coordinates.WorkY,
                StartZ = state.Coordinates.WorkZ
            };

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                interpreted.IsIgnored = true;
                interpreted.ModalStateAfterLine = state.Clone();
                result.Commands.Add(interpreted);
                continue;
            }

            var tokens = TokenRegex.Matches(sanitized.ToUpperInvariant());
            var seenAxisWord = false;
            var endX = state.Coordinates.WorkX;
            var endY = state.Coordinates.WorkY;
            var endZ = state.Coordinates.WorkZ;
            decimal? feedRate = null;
            decimal? spindleSpeed = null;
            int? toolNumber = null;
            decimal? arcI = null;
            decimal? arcJ = null;
            decimal? arcK = null;
            var unsupportedMessages = new List<string>();

            foreach (Match token in tokens)
            {
                if (!token.Success)
                    continue;

                var prefix = token.Groups[1].Value.ToUpperInvariant();
                var valueText = token.Groups[2].Value;
                if (!decimal.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    continue;

                switch (prefix)
                {
                    case "G":
                        ApplyGCode((int)value, state, unsupportedMessages);
                        break;
                    case "M":
                        ApplyMCode((int)value, state, interpreted, unsupportedMessages);
                        break;
                    case "X":
                        endX = ResolveAxisTarget(state, value, state.Coordinates.WorkX);
                        seenAxisWord = true;
                        break;
                    case "Y":
                        endY = ResolveAxisTarget(state, value, state.Coordinates.WorkY);
                        seenAxisWord = true;
                        break;
                    case "Z":
                        endZ = ResolveAxisTarget(state, value, state.Coordinates.WorkZ);
                        seenAxisWord = true;
                        break;
                    case "I":
                        arcI = ConvertLinearValue(state.Units, value);
                        break;
                    case "J":
                        arcJ = ConvertLinearValue(state.Units, value);
                        break;
                    case "K":
                        arcK = ConvertLinearValue(state.Units, value);
                        break;
                    case "F":
                        feedRate = ConvertLinearValue(state.Units, value);
                        state.FeedRateMmPerMinute = feedRate.Value;
                        break;
                    case "S":
                        spindleSpeed = value;
                        state.SpindleSpeed = value;
                        break;
                    case "T":
                        toolNumber = (int)value;
                        state.ToolNumber = (int)value;
                        break;
                }
            }

            interpreted.Units = state.Units;
            interpreted.DistanceMode = state.DistanceMode;
            interpreted.MotionMode = state.MotionMode;
            interpreted.Plane = state.Plane;
            interpreted.SpindleState = state.SpindleState;
            interpreted.SpindleSpeed = spindleSpeed ?? state.SpindleSpeed;
            interpreted.ToolNumber = toolNumber ?? state.ToolNumber;
            interpreted.FeedRateMmPerMinute = feedRate ?? (state.FeedRateMmPerMinute > 0m ? state.FeedRateMmPerMinute : null);
            interpreted.ArcOffsetI = arcI;
            interpreted.ArcOffsetJ = arcJ;
            interpreted.ArcOffsetK = arcK;

            if (unsupportedMessages.Count > 0)
            {
                interpreted.IsUnsupported = true;
                interpreted.BlocksExecution = true;
                interpreted.DiagnosticMessage = string.Join(" ", unsupportedMessages);
                foreach (var message in unsupportedMessages)
                {
                    result.Diagnostics.UnsupportedCommandCount++;
                    if (message.Contains("plane", StringComparison.OrdinalIgnoreCase))
                        result.Diagnostics.UnsupportedPlaneCount++;
                    result.Diagnostics.AddMessage("Error", $"Line {lineNumber}: {message}");
                }
            }

            if (seenAxisWord)
            {
                if (state.MotionMode == GcodeModalMotionMode.None)
                {
                    interpreted.BlocksExecution = true;
                    interpreted.DiagnosticMessage = "Axis words were found without an active motion mode.";
                    result.Diagnostics.AddMessage("Error", $"Line {lineNumber}: axis words were found without an active motion mode.");
                }
                else
                {
                    interpreted.HasMotion = true;
                    interpreted.EmitsControllerCommand = true;
                    interpreted.StartX = state.Coordinates.WorkX;
                    interpreted.StartY = state.Coordinates.WorkY;
                    interpreted.StartZ = state.Coordinates.WorkZ;
                    interpreted.EndX = endX;
                    interpreted.EndY = endY;
                    interpreted.EndZ = endZ;

                    if (state.MotionMode is GcodeModalMotionMode.ArcClockwise or GcodeModalMotionMode.ArcCounterClockwise)
                        ApplyArcMetadata(interpreted, result.Diagnostics);

                    state.Coordinates.WorkX = endX;
                    state.Coordinates.WorkY = endY;
                    state.Coordinates.WorkZ = endZ;
                }
            }
            else if (interpreted.IsSpindleChange)
            {
                interpreted.EmitsControllerCommand = true;
            }
            else if (feedRate.HasValue || spindleSpeed.HasValue || toolNumber.HasValue)
            {
                interpreted.EmitsControllerCommand = false;
            }

            if (!interpreted.HasMotion && !interpreted.EmitsControllerCommand && !interpreted.IsUnsupported)
                interpreted.IsIgnored = true;

            interpreted.ModalStateAfterLine = state.Clone();
            result.Commands.Add(interpreted);
        }

        result.FinalModalState = state.Clone();
        return result;
    }

    private static void ApplyArcMetadata(GcodeInterpretedCommand command, GcodeInterpreterDiagnostics diagnostics)
    {
        if (command.Plane != GcodePlane.XY)
        {
            command.IsUnsupported = true;
            command.BlocksExecution = true;
            command.DiagnosticMessage = "Only XY plane arcs (G17) are supported currently.";
            diagnostics.UnsupportedPlaneCount++;
            diagnostics.UnsupportedCommandCount++;
            diagnostics.AddMessage("Error", $"Line {command.SourceLineNumber}: only XY plane arcs (G17) are supported currently.");
            return;
        }

        if (command.ArcOffsetI == null && command.ArcOffsetJ == null)
        {
            command.IsUnsupported = true;
            command.BlocksExecution = true;
            command.DiagnosticMessage = "Arc motion requires I/J center offsets.";
            diagnostics.UnsupportedCommandCount++;
            diagnostics.AddMessage("Error", $"Line {command.SourceLineNumber}: arc motion requires I/J center offsets.");
            return;
        }

        var centerX = command.StartX + (command.ArcOffsetI ?? 0m);
        var centerY = command.StartY + (command.ArcOffsetJ ?? 0m);
        command.ArcCenterX = centerX;
        command.ArcCenterY = centerY;
        command.ArcCenterZ = command.StartZ;
        command.ArcRadiusMm = (decimal)Math.Sqrt((double)(((command.StartX - centerX) * (command.StartX - centerX)) + ((command.StartY - centerY) * (command.StartY - centerY))));
        var startAngle = Math.Atan2((double)(command.StartY - centerY), (double)(command.StartX - centerX));
        var endAngle = Math.Atan2((double)(command.EndY - centerY), (double)(command.EndX - centerX));
        var clockwise = command.MotionMode == GcodeModalMotionMode.ArcClockwise;
        var sweep = NormalizeSweep(startAngle, endAngle, clockwise);
        command.ArcLengthMm = (decimal)Math.Abs(sweep) * (command.ArcRadiusMm ?? 0m);
    }

    private static void ApplyGCode(int gCode, GcodeModalState state, List<string> unsupportedMessages)
    {
        switch (gCode)
        {
            case 0:
                state.MotionMode = GcodeModalMotionMode.Rapid;
                break;
            case 1:
                state.MotionMode = GcodeModalMotionMode.Linear;
                break;
            case 2:
                state.MotionMode = GcodeModalMotionMode.ArcClockwise;
                break;
            case 3:
                state.MotionMode = GcodeModalMotionMode.ArcCounterClockwise;
                break;
            case 17:
                state.Plane = GcodePlane.XY;
                break;
            case 18:
                state.Plane = GcodePlane.XZ;
                unsupportedMessages.Add("G18/XZ plane is not supported for execution yet.");
                break;
            case 19:
                state.Plane = GcodePlane.YZ;
                unsupportedMessages.Add("G19/YZ plane is not supported for execution yet.");
                break;
            case 20:
                state.Units = GcodeUnitMode.Inches;
                break;
            case 21:
                state.Units = GcodeUnitMode.Millimeters;
                break;
            case 54:
                state.CurrentWorkCoordinateSystem = 54;
                break;
            case 55:
            case 56:
            case 57:
            case 58:
            case 59:
                unsupportedMessages.Add($"G{gCode} work coordinate systems are not supported yet.");
                break;
            case 90:
                state.DistanceMode = GcodeDistanceMode.Absolute;
                break;
            case 91:
                state.DistanceMode = GcodeDistanceMode.Incremental;
                break;
            case 80:
                break;
            case 81:
            case 82:
            case 83:
            case 84:
            case 85:
            case 86:
            case 87:
            case 88:
            case 89:
                unsupportedMessages.Add($"G{gCode} canned cycles are not supported yet.");
                break;
            default:
                if (gCode != 4)
                    unsupportedMessages.Add($"G{gCode} is not supported by the current interpreter.");
                break;
        }
    }

    private static void ApplyMCode(int mCode, GcodeModalState state, GcodeInterpretedCommand interpreted, List<string> unsupportedMessages)
    {
        switch (mCode)
        {
            case 3:
                state.SpindleState = GcodeSpindleState.Clockwise;
                interpreted.IsSpindleChange = true;
                break;
            case 5:
                state.SpindleState = GcodeSpindleState.Off;
                interpreted.IsSpindleChange = true;
                break;
            default:
                unsupportedMessages.Add($"M{mCode} is not supported by the current interpreter.");
                break;
        }
    }

    private static decimal ResolveAxisTarget(GcodeModalState state, decimal rawValue, decimal currentValue)
    {
        var converted = ConvertLinearValue(state.Units, rawValue);
        return state.DistanceMode == GcodeDistanceMode.Absolute
            ? converted
            : currentValue + converted;
    }

    private static decimal ConvertLinearValue(GcodeUnitMode units, decimal rawValue)
    {
        return units == GcodeUnitMode.Inches
            ? rawValue * 25.4m
            : rawValue;
    }

    private static string StripComments(string line)
    {
        var withoutBracketComments = InlineCommentRegex.Replace(line, string.Empty);
        var semicolonIndex = withoutBracketComments.IndexOf(';');
        return semicolonIndex >= 0
            ? withoutBracketComments[..semicolonIndex]
            : withoutBracketComments;
    }

    private static string? ExtractComment(string line)
    {
        var comments = new List<string>();
        var inlineMatches = InlineCommentRegex.Matches(line);
        foreach (Match match in inlineMatches)
        {
            if (match.Success)
                comments.Add(match.Value.Trim('(', ')', ' '));
        }

        var semicolonIndex = line.IndexOf(';');
        if (semicolonIndex >= 0 && semicolonIndex < line.Length - 1)
            comments.Add(line[(semicolonIndex + 1)..].Trim());

        return comments.Count == 0 ? null : string.Join(" | ", comments.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    private static double NormalizeSweep(double startAngle, double endAngle, bool clockwise)
    {
        var sweep = endAngle - startAngle;
        if (clockwise && sweep >= 0d)
            sweep -= 2d * Math.PI;
        else if (!clockwise && sweep <= 0d)
            sweep += 2d * Math.PI;

        return sweep;
    }
}

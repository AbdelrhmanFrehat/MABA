using System.IO;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class GcodeParserService : IGcodeParserService
{
    private readonly GcodeInterpreterService _interpreterService;

    public GcodeParserService()
        : this(new GcodeInterpreterService())
    {
    }

    public GcodeParserService(GcodeInterpreterService interpreterService)
    {
        _interpreterService = interpreterService;
    }

    public GcodeParseResult ParseFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var interpreted = _interpreterService.Interpret(lines);
        var result = new GcodeParseResult
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            TotalLines = lines.Length,
            FinalModalState = interpreted.FinalModalState,
            InterpreterDiagnostics = interpreted.Diagnostics,
            WarningCount = interpreted.Diagnostics.WarningCount,
            ErrorCount = interpreted.Diagnostics.ErrorCount,
            UnsupportedCommandCount = interpreted.Diagnostics.UnsupportedCommandCount
        };

        foreach (var message in interpreted.Diagnostics.Messages)
            result.Messages.Add(message);

        foreach (var command in interpreted.Commands)
        {
            result.InterpretedCommands.Add(command);
            if (!command.HasMotion || command.MotionType == null)
                continue;

            result.Motions.Add(new GcodeMotionCommand
            {
                LineNumber = command.SourceLineNumber,
                RawText = command.RawLine,
                MotionType = command.MotionType.Value,
                StartX = command.StartX,
                StartY = command.StartY,
                StartZ = command.StartZ,
                EndX = command.EndX,
                EndY = command.EndY,
                EndZ = command.EndZ,
                IsAbsoluteMode = command.DistanceMode == GcodeDistanceMode.Absolute,
                FeedRate = command.FeedRateMmPerMinute,
                Units = command.Units,
                DistanceMode = command.DistanceMode,
                Plane = command.Plane,
                ArcOffsetI = command.ArcOffsetI,
                ArcOffsetJ = command.ArcOffsetJ,
                ArcOffsetK = command.ArcOffsetK,
                ArcCenterX = command.ArcCenterX,
                ArcCenterY = command.ArcCenterY,
                ArcCenterZ = command.ArcCenterZ,
                ArcRadiusMm = command.ArcRadiusMm,
                ArcLengthMm = command.ArcLengthMm,
                ModalSummary = $"{FormatUnits(command.Units)} / {FormatDistanceMode(command.DistanceMode)} / {FormatPlane(command.Plane)}",
                IsValid = !command.BlocksExecution,
                ValidationMessage = command.BlocksExecution ? command.DiagnosticMessage : null
            });
        }

        result.Messages.Add($"[Info] Parse finished: {result.Motions.Count} interpreted motion line(s) loaded.");
        return result;
    }

    private static string FormatUnits(GcodeUnitMode units)
    {
        return units == GcodeUnitMode.Inches ? "inch" : "mm";
    }

    private static string FormatDistanceMode(GcodeDistanceMode mode)
    {
        return mode == GcodeDistanceMode.Incremental ? "G91" : "G90";
    }

    private static string FormatPlane(GcodePlane plane)
    {
        return plane switch
        {
            GcodePlane.XZ => "G18",
            GcodePlane.YZ => "G19",
            _ => "G17"
        };
    }
}

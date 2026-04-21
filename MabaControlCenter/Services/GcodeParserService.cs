using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class GcodeParserService : IGcodeParserService
{
    private static readonly Regex TokenRegex = new(@"([A-Za-z])([+-]?(?:\d+\.?\d*|\.\d+))", RegexOptions.Compiled);
    private static readonly Regex InlineCommentRegex = new(@"\(.*?\)", RegexOptions.Compiled);

    public GcodeParseResult ParseFile(string filePath)
    {
        var result = new GcodeParseResult
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        };

        var lines = File.ReadAllLines(filePath);
        result.TotalLines = lines.Length;

        var absoluteMode = true;
        var unitScale = 1m; // mm
        var currentX = 0m;
        var currentY = 0m;
        var currentZ = 0m;

        for (var index = 0; index < lines.Length; index++)
        {
            var rawLine = lines[index];
            var lineNumber = index + 1;
            var sanitized = StripComments(rawLine).Trim();

            if (string.IsNullOrWhiteSpace(sanitized))
                continue;

            var upper = sanitized.ToUpperInvariant();
            var tokens = TokenRegex.Matches(upper);
            var gCodes = new List<int>();

            foreach (Match token in tokens)
            {
                if (!token.Success)
                    continue;

                var prefix = token.Groups[1].Value.ToUpperInvariant();
                if (prefix != "G")
                    continue;

                if (decimal.TryParse(token.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var gValue))
                    gCodes.Add((int)gValue);
            }

            if (gCodes.Contains(20))
                unitScale = 25.4m;
            if (gCodes.Contains(21))
                unitScale = 1m;
            if (gCodes.Contains(90))
                absoluteMode = true;
            if (gCodes.Contains(91))
                absoluteMode = false;

            var motionType = GetMotionType(gCodes);
            if (motionType == null)
            {
                if (HasUnsupportedMotionCode(gCodes))
                    AddMessage(result, $"Line {lineNumber}: unsupported motion command ignored.", isError: false);
                continue;
            }

            decimal? targetX = null;
            decimal? targetY = null;
            decimal? targetZ = null;
            decimal? feed = null;

            foreach (Match token in tokens)
            {
                var prefix = token.Groups[1].Value;
                var valueText = token.Groups[2].Value;

                if (!decimal.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    continue;

                switch (prefix)
                {
                    case "X":
                        targetX = absoluteMode ? value * unitScale : currentX + (value * unitScale);
                        break;
                    case "Y":
                        targetY = absoluteMode ? value * unitScale : currentY + (value * unitScale);
                        break;
                    case "Z":
                        targetZ = absoluteMode ? value * unitScale : currentZ + (value * unitScale);
                        break;
                    case "F":
                        feed = value;
                        break;
                }
            }

            if (targetX == null && targetY == null && targetZ == null)
            {
                AddMessage(result, $"Line {lineNumber}: motion command has no X/Y/Z move and was ignored.", isError: false);
                continue;
            }

            var motion = new GcodeMotionCommand
            {
                LineNumber = lineNumber,
                RawText = rawLine,
                MotionType = motionType.Value,
                StartX = currentX,
                StartY = currentY,
                StartZ = currentZ,
                EndX = targetX ?? currentX,
                EndY = targetY ?? currentY,
                EndZ = targetZ ?? currentZ,
                IsAbsoluteMode = absoluteMode,
                FeedRate = feed
            };

            result.Motions.Add(motion);
            currentX = motion.EndX;
            currentY = motion.EndY;
            currentZ = motion.EndZ;
        }

        AddMessage(result, $"Parse finished: {result.Motions.Count} motion line(s) loaded.", isError: false);
        return result;
    }

    private static string StripComments(string line)
    {
        var withoutBracketComments = InlineCommentRegex.Replace(line, string.Empty);
        var semicolonIndex = withoutBracketComments.IndexOf(';');
        return semicolonIndex >= 0
            ? withoutBracketComments[..semicolonIndex]
            : withoutBracketComments;
    }

    private static GcodeMotionType? GetMotionType(IEnumerable<int> gCodes)
    {
        var codes = gCodes.ToList();
        if (codes.Contains(0))
            return GcodeMotionType.Rapid;
        if (codes.Contains(1))
            return GcodeMotionType.Linear;
        return null;
    }

    private static bool HasUnsupportedMotionCode(IEnumerable<int> gCodes)
    {
        var codes = gCodes.ToList();
        return codes.Contains(2) || codes.Contains(3);
    }

    private static void AddMessage(GcodeParseResult result, string message, bool isError)
    {
        result.Messages.Add(message);
        if (isError)
            result.ErrorCount++;
        else
            result.WarningCount++;
    }
}

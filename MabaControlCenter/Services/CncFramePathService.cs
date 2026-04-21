using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncFramePathService : ICncFramePathService
{
    public CncFrameBounds CalculateBounds(IReadOnlyList<GcodeMotionCommand> motions)
    {
        var executable = motions.Where(m => m.IsExecutable).ToList();
        if (executable.Count == 0)
            return new CncFrameBounds();

        var points = executable
            .SelectMany(m => new[]
            {
                new { m.StartX, m.StartY },
                new { StartX = m.EndX, StartY = m.EndY }
            })
            .ToList();

        return new CncFrameBounds
        {
            MinX = points.Min(p => p.StartX),
            MaxX = points.Max(p => p.StartX),
            MinY = points.Min(p => p.StartY),
            MaxY = points.Max(p => p.StartY)
        };
    }

    public IReadOnlyList<GcodeMotionCommand> BuildFramePath(CncFrameBounds bounds)
    {
        if (!bounds.IsValid)
            return Array.Empty<GcodeMotionCommand>();

        var corners = new[]
        {
            (X: bounds.MinX, Y: bounds.MinY),
            (X: bounds.MaxX, Y: bounds.MinY),
            (X: bounds.MaxX, Y: bounds.MaxY),
            (X: bounds.MinX, Y: bounds.MaxY),
            (X: bounds.MinX, Y: bounds.MinY)
        };

        var frameMotions = new List<GcodeMotionCommand>();
        for (var i = 0; i < corners.Length - 1; i++)
        {
            frameMotions.Add(new GcodeMotionCommand
            {
                LineNumber = i + 1,
                RawText = $"FRAME {i + 1}",
                MotionType = GcodeMotionType.Rapid,
                StartX = corners[i].X,
                StartY = corners[i].Y,
                StartZ = 0m,
                EndX = corners[i + 1].X,
                EndY = corners[i + 1].Y,
                EndZ = 0m,
                IsAbsoluteMode = true,
                FeedRate = 6000m
            });
        }

        return frameMotions;
    }
}

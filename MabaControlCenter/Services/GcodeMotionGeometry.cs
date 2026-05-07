using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public static class GcodeMotionGeometry
{
    public static (decimal X, decimal Y, decimal Z) GetPointAtProgress(GcodeMotionCommand motion, double progress)
    {
        var clamped = Math.Clamp(progress, 0d, 1d);
        if (!motion.IsArcMove || motion.ArcCenterX == null || motion.ArcCenterY == null)
        {
            return (
                Interpolate(motion.StartX, motion.EndX, clamped),
                Interpolate(motion.StartY, motion.EndY, clamped),
                Interpolate(motion.StartZ, motion.EndZ, clamped));
        }

        var centerX = motion.ArcCenterX.Value;
        var centerY = motion.ArcCenterY.Value;
        var radius = motion.ArcRadiusMm ?? Distance(centerX, centerY, motion.StartX, motion.StartY);
        var startAngle = Math.Atan2((double)(motion.StartY - centerY), (double)(motion.StartX - centerX));
        var endAngle = Math.Atan2((double)(motion.EndY - centerY), (double)(motion.EndX - centerX));
        var clockwise = motion.MotionType == GcodeMotionType.ArcClockwise;
        var sweep = NormalizeSweep(startAngle, endAngle, clockwise);
        var angle = startAngle + (sweep * clamped);

        return (
            centerX + ((decimal)Math.Cos(angle) * radius),
            centerY + ((decimal)Math.Sin(angle) * radius),
            Interpolate(motion.StartZ, motion.EndZ, clamped));
    }

    public static IReadOnlyList<(decimal X, decimal Y, decimal Z)> SamplePath(GcodeMotionCommand motion, int minimumSegments = 24)
    {
        if (!motion.IsArcMove || motion.ArcCenterX == null || motion.ArcCenterY == null)
            return new List<(decimal X, decimal Y, decimal Z)> { (motion.StartX, motion.StartY, motion.StartZ), (motion.EndX, motion.EndY, motion.EndZ) };

        var segments = Math.Max(minimumSegments, (int)Math.Ceiling((double)(motion.LengthMm / 2m)));
        var points = new List<(decimal X, decimal Y, decimal Z)>(segments + 1);
        for (var i = 0; i <= segments; i++)
        {
            var progress = i / (double)segments;
            points.Add(GetPointAtProgress(motion, progress));
        }

        return points;
    }

    private static decimal Interpolate(decimal start, decimal end, double progress)
    {
        return start + ((end - start) * (decimal)progress);
    }

    private static decimal Distance(decimal x1, decimal y1, decimal x2, decimal y2)
    {
        return (decimal)Math.Sqrt((double)(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1))));
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

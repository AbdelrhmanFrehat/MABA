using System.Windows;

namespace MabaControlCenter.Controls;

internal sealed class CncViewportTransform
{
    private const double LeftRulerWidth = 44d;
    private const double BottomRulerHeight = 30d;
    private const double TopPadding = 16d;
    private const double RightPadding = 16d;

    public CncViewportTransform(Rect frame, double minX, double maxX, double minY, double maxY)
    {
        MinX = minX;
        MaxX = Math.Max(minX + 1d, maxX);
        MinY = minY;
        MaxY = Math.Max(minY + 1d, maxY);

        WorkspaceWidthMm = Math.Max(1d, MaxX - MinX);
        WorkspaceHeightMm = Math.Max(1d, MaxY - MinY);

        var contentRect = new Rect(
            frame.Left + LeftRulerWidth,
            frame.Top + TopPadding,
            Math.Max(32d, frame.Width - LeftRulerWidth - RightPadding),
            Math.Max(32d, frame.Height - TopPadding - BottomRulerHeight));

        Scale = Math.Min(contentRect.Width / WorkspaceWidthMm, contentRect.Height / WorkspaceHeightMm);
        BedRect = new Rect(
            contentRect.Left + ((contentRect.Width - (WorkspaceWidthMm * Scale)) / 2d),
            contentRect.Top + ((contentRect.Height - (WorkspaceHeightMm * Scale)) / 2d),
            WorkspaceWidthMm * Scale,
            WorkspaceHeightMm * Scale);
    }

    public double MinX { get; }
    public double MaxX { get; }
    public double MinY { get; }
    public double MaxY { get; }
    public double WorkspaceWidthMm { get; }
    public double WorkspaceHeightMm { get; }
    public double Scale { get; }
    public Rect BedRect { get; }
    public Rect OuterFrame => new(
        BedRect.Left - LeftRulerWidth,
        BedRect.Top - TopPadding,
        BedRect.Width + LeftRulerWidth + RightPadding,
        BedRect.Height + TopPadding + BottomRulerHeight);

    public Point Map(double xMm, double yMm)
    {
        var clampedX = Math.Max(MinX, Math.Min(MaxX, xMm));
        var clampedY = Math.Max(MinY, Math.Min(MaxY, yMm));
        var screenX = BedRect.Left + ((clampedX - MinX) * Scale);
        var screenY = BedRect.Bottom - ((clampedY - MinY) * Scale);
        return new Point(screenX, screenY);
    }

    public double GetMinorSpacingMm()
    {
        return Scale * 10d >= 10d ? 10d : 0d;
    }

    public double GetMajorSpacingMm()
    {
        if (Scale * 50d >= 36d)
            return 50d;

        if (Scale * 100d >= 36d)
            return 100d;

        return 200d;
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.Controls;

public class CncToolpathPreviewControl : FrameworkElement
{
    public static readonly DependencyProperty MotionsProperty =
        DependencyProperty.Register(
            nameof(Motions),
            typeof(IEnumerable<GcodeMotionCommand>),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentSegmentIndexProperty =
        DependencyProperty.Register(
            nameof(CurrentSegmentIndex),
            typeof(int),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SegmentProgressProperty =
        DependencyProperty.Register(
            nameof(SegmentProgress),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ToolXProperty =
        DependencyProperty.Register(
            nameof(ToolX),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ToolYProperty =
        DependencyProperty.Register(
            nameof(ToolY),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineMinXProperty =
        DependencyProperty.Register(
            nameof(MachineMinX),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineMaxXProperty =
        DependencyProperty.Register(
            nameof(MachineMaxX),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineMinYProperty =
        DependencyProperty.Register(
            nameof(MachineMinY),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineMaxYProperty =
        DependencyProperty.Register(
            nameof(MachineMaxY),
            typeof(double),
            typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable<GcodeMotionCommand>? Motions
    {
        get => (IEnumerable<GcodeMotionCommand>?)GetValue(MotionsProperty);
        set => SetValue(MotionsProperty, value);
    }

    public int CurrentSegmentIndex
    {
        get => (int)GetValue(CurrentSegmentIndexProperty);
        set => SetValue(CurrentSegmentIndexProperty, value);
    }

    public double SegmentProgress
    {
        get => (double)GetValue(SegmentProgressProperty);
        set => SetValue(SegmentProgressProperty, value);
    }

    public double ToolX
    {
        get => (double)GetValue(ToolXProperty);
        set => SetValue(ToolXProperty, value);
    }

    public double ToolY
    {
        get => (double)GetValue(ToolYProperty);
        set => SetValue(ToolYProperty, value);
    }

    public double MachineMinX
    {
        get => (double)GetValue(MachineMinXProperty);
        set => SetValue(MachineMinXProperty, value);
    }

    public double MachineMaxX
    {
        get => (double)GetValue(MachineMaxXProperty);
        set => SetValue(MachineMaxXProperty, value);
    }

    public double MachineMinY
    {
        get => (double)GetValue(MachineMinYProperty);
        set => SetValue(MachineMinYProperty, value);
    }

    public double MachineMaxY
    {
        get => (double)GetValue(MachineMaxYProperty);
        set => SetValue(MachineMaxYProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var rect = new Rect(0, 0, ActualWidth, ActualHeight);
        var background = ThemeColor("BackgroundLight", Color.FromRgb(0x18, 0x22, 0x33));
        var surface = ThemeColor("BackgroundMedium", Color.FromRgb(0x0D, 0x15, 0x20));
        var border = ThemeColor("BorderColor", Color.FromRgb(0x51, 0x63, 0x78));
        dc.DrawRectangle(new LinearGradientBrush(background, surface, 90), null, rect);

        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        var frameRect = new Rect(12, 12, Math.Max(0, ActualWidth - 24), Math.Max(0, ActualHeight - 24));
        var transform = new CncViewportTransform(frameRect, MachineMinX, MachineMaxX, MachineMinY, MachineMaxY);
        DrawViewportChrome(dc, transform, surface, border);

        var motions = Motions?.Where(m => m.IsExecutable).ToList() ?? new List<GcodeMotionCommand>();

        var cutPen = new Pen(new SolidColorBrush(Color.FromRgb(0x4D, 0xD0, 0x9E)), 1.9) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var rapidPen = new Pen(new SolidColorBrush(Color.FromRgb(0xF0, 0x6A, 0x6A)), 1.0) { DashStyle = DashStyles.Dash, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var completedCutPen = new Pen(new SolidColorBrush(Color.FromRgb(0x94, 0xFF, 0xC8)), 2.4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var completedRapidPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0xAA)), 1.4) { DashStyle = DashStyles.Dash, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var remainingCutPen = new Pen(new SolidColorBrush(Color.FromArgb(0x77, 0x35, 0xC0, 0x86)), 1.45) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var remainingRapidPen = new Pen(new SolidColorBrush(Color.FromArgb(0x99, 0xE2, 0x6C, 0x6C)), 0.9) { DashStyle = DashStyles.Dot, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var currentPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x49)), 2.8) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        for (var i = 0; i < motions.Count; i++)
        {
            var motion = motions[i];

            Pen statePen;
            if (CurrentSegmentIndex >= 0 && i < CurrentSegmentIndex)
            {
                statePen = motion.IsRapidMove ? completedRapidPen : completedCutPen;
            }
            else if (i == CurrentSegmentIndex)
            {
                statePen = currentPen;
            }
            else
            {
                statePen = motion.IsRapidMove ? remainingRapidPen : remainingCutPen;
            }

            DrawMotion(dc, motion, transform, motion.IsRapidMove ? rapidPen : cutPen);
            DrawMotion(dc, motion, transform, statePen);

            if (i == CurrentSegmentIndex && SegmentProgress > 0d && SegmentProgress < 1d)
            {
                DrawPartialMotion(dc, motion, transform, currentPen, SegmentProgress);
            }
        }

        var toolPoint = transform.Map(ToolX, ToolY);
        var toolFill = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
        var toolStroke = new Pen(new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), 1.5);
        dc.DrawEllipse(toolFill, toolStroke, toolPoint, 5, 5);
        dc.DrawLine(toolStroke, new Point(toolPoint.X - 8, toolPoint.Y), new Point(toolPoint.X + 8, toolPoint.Y));
        dc.DrawLine(toolStroke, new Point(toolPoint.X, toolPoint.Y - 8), new Point(toolPoint.X, toolPoint.Y + 8));
    }

    private void DrawViewportChrome(DrawingContext dc, CncViewportTransform transform, Color surface, Color border)
    {
        dc.DrawRectangle(new SolidColorBrush(WithAlpha(surface, 0x55)), new Pen(new SolidColorBrush(border), 1.1), transform.BedRect);
        DrawGridAndRulers(dc, transform);
        DrawOriginAndAxes(dc, transform);
        DrawLabel(
            dc,
            $"Workspace: {transform.WorkspaceWidthMm:0.#} x {transform.WorkspaceHeightMm:0.#} mm",
            new Point(transform.BedRect.Left, transform.BedRect.Top - 20),
            ThemeColor("TextPrimary", Color.FromRgb(0xE2, 0xE8, 0xF0)),
            11.5,
            FontWeights.SemiBold);
    }

    private void DrawGridAndRulers(DrawingContext dc, CncViewportTransform transform)
    {
        var minorSpacing = transform.GetMinorSpacingMm();
        var majorSpacing = transform.GetMajorSpacingMm();
        var minorPen = new Pen(new SolidColorBrush(WithAlpha(ThemeColor("BorderColor", Color.FromRgb(0xBD, 0xC8, 0xD4)), 0x2E)), 0.6);
        var majorPen = new Pen(new SolidColorBrush(WithAlpha(ThemeColor("BorderColor", Color.FromRgb(0xBD, 0xC8, 0xD4)), 0x66)), 0.95);

        if (minorSpacing > 0d)
        {
            for (var x = transform.MinX + minorSpacing; x < transform.MaxX; x += minorSpacing)
            {
                var p = transform.Map(x, transform.MinY);
                dc.DrawLine(minorPen, new Point(p.X, transform.BedRect.Top), new Point(p.X, transform.BedRect.Bottom));
            }

            for (var y = transform.MinY + minorSpacing; y < transform.MaxY; y += minorSpacing)
            {
                var p = transform.Map(transform.MinX, y);
                dc.DrawLine(minorPen, new Point(transform.BedRect.Left, p.Y), new Point(transform.BedRect.Right, p.Y));
            }
        }

        var firstMajorX = Math.Ceiling(transform.MinX / majorSpacing) * majorSpacing;
        for (var x = firstMajorX; x <= transform.MaxX + 0.001d; x += majorSpacing)
        {
            var p = transform.Map(x, transform.MinY);
            dc.DrawLine(majorPen, new Point(p.X, transform.BedRect.Top), new Point(p.X, transform.BedRect.Bottom));
            DrawLabel(dc, $"{x:0}", new Point(p.X - 10, transform.BedRect.Bottom + 6), ThemeColor("TextSecondary", Color.FromRgb(0x94, 0xA3, 0xB8)), 10.5, FontWeights.Medium);
        }

        var firstMajorY = Math.Ceiling(transform.MinY / majorSpacing) * majorSpacing;
        for (var y = firstMajorY; y <= transform.MaxY + 0.001d; y += majorSpacing)
        {
            var p = transform.Map(transform.MinX, y);
            dc.DrawLine(majorPen, new Point(transform.BedRect.Left, p.Y), new Point(transform.BedRect.Right, p.Y));
            DrawLabel(dc, $"{y:0}", new Point(transform.BedRect.Left - 34, p.Y - 7), ThemeColor("TextSecondary", Color.FromRgb(0x94, 0xA3, 0xB8)), 10.5, FontWeights.Medium);
        }
    }

    private void DrawOriginAndAxes(DrawingContext dc, CncViewportTransform transform)
    {
        var origin = transform.Map(transform.MinX, transform.MinY);
        var originPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFB, 0x71, 0x71)), 1.5);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x40, 0xFB, 0x71, 0x71)), originPen, origin, 5.5, 5.5);
        dc.DrawLine(originPen, new Point(origin.X - 8, origin.Y), new Point(origin.X + 8, origin.Y));
        dc.DrawLine(originPen, new Point(origin.X, origin.Y - 8), new Point(origin.X, origin.Y + 8));

        var xArrowEnd = new Point(Math.Min(transform.BedRect.Right, origin.X + 36), origin.Y);
        var yArrowEnd = new Point(origin.X, Math.Max(transform.BedRect.Top, origin.Y - 36));
        var axisPen = new Pen(new SolidColorBrush(Color.FromRgb(0x93, 0xC5, 0xFD)), 1.4);
        dc.DrawLine(axisPen, origin, xArrowEnd);
        dc.DrawLine(axisPen, origin, yArrowEnd);
        DrawArrowHead(dc, axisPen, xArrowEnd, new Vector(1, 0));
        DrawArrowHead(dc, axisPen, yArrowEnd, new Vector(0, -1));
        DrawLabel(dc, "X", new Point(xArrowEnd.X + 6, xArrowEnd.Y - 10), Color.FromRgb(0x93, 0xC5, 0xFD), 10.5, FontWeights.SemiBold);
        DrawLabel(dc, "Y", new Point(yArrowEnd.X + 6, yArrowEnd.Y - 10), Color.FromRgb(0x93, 0xC5, 0xFD), 10.5, FontWeights.SemiBold);
    }

    private static void DrawArrowHead(DrawingContext dc, Pen pen, Point tip, Vector direction)
    {
        direction.Normalize();
        var left = new Vector(-direction.Y, direction.X);
        var back = -direction * 8d;
        var wing = left * 4d;
        dc.DrawLine(pen, tip, tip + back + wing);
        dc.DrawLine(pen, tip, tip + back - wing);
    }

    private Color ThemeColor(string key, Color fallback)
    {
        if (TryFindResource(key) is Color color)
            return color;

        if (TryFindResource($"{key}Brush") is SolidColorBrush brush)
            return brush.Color;

        return fallback;
    }

    private static Color WithAlpha(Color color, byte alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    private static void DrawMotion(DrawingContext dc, GcodeMotionCommand motion, CncViewportTransform transform, Pen pen)
    {
        var points = GcodeMotionGeometry.SamplePath(motion);
        for (var i = 0; i < points.Count - 1; i++)
        {
            dc.DrawLine(
                pen,
                transform.Map((double)points[i].X, (double)points[i].Y),
                transform.Map((double)points[i + 1].X, (double)points[i + 1].Y));
        }
    }

    private static void DrawPartialMotion(DrawingContext dc, GcodeMotionCommand motion, CncViewportTransform transform, Pen pen, double progress)
    {
        var sampled = GcodeMotionGeometry.SamplePath(motion);
        var visibleSegments = Math.Clamp((int)Math.Ceiling((sampled.Count - 1) * progress), 1, sampled.Count - 1);
        for (var i = 0; i < visibleSegments; i++)
        {
            dc.DrawLine(
                pen,
                transform.Map((double)sampled[i].X, (double)sampled[i].Y),
                transform.Map((double)sampled[i + 1].X, (double)sampled[i + 1].Y));
        }
    }

    private void DrawLabel(DrawingContext dc, string text, Point position, Color color, double fontSize, FontWeight weight)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal),
            fontSize,
            new SolidColorBrush(color),
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(formatted, position);
    }
}

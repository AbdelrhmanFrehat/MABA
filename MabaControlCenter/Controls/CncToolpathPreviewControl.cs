using System.Windows;
using System.Windows.Media;
using MabaControlCenter.Models;

namespace MabaControlCenter.Controls;

public class CncToolpathPreviewControl : FrameworkElement
{
    public static readonly DependencyProperty MotionsProperty =
        DependencyProperty.Register(nameof(Motions), typeof(IEnumerable<GcodeMotionCommand>), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentSegmentIndexProperty =
        DependencyProperty.Register(nameof(CurrentSegmentIndex), typeof(int), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SegmentProgressProperty =
        DependencyProperty.Register(nameof(SegmentProgress), typeof(double), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ToolXProperty =
        DependencyProperty.Register(nameof(ToolX), typeof(double), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ToolYProperty =
        DependencyProperty.Register(nameof(ToolY), typeof(double), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineWidthProperty =
        DependencyProperty.Register(nameof(MachineWidth), typeof(double), typeof(CncToolpathPreviewControl),
            new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MachineHeightProperty =
        DependencyProperty.Register(nameof(MachineHeight), typeof(double), typeof(CncToolpathPreviewControl),
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

    public double MachineWidth
    {
        get => (double)GetValue(MachineWidthProperty);
        set => SetValue(MachineWidthProperty, value);
    }

    public double MachineHeight
    {
        get => (double)GetValue(MachineHeightProperty);
        set => SetValue(MachineHeightProperty, value);
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

        var frameRect = new Rect(14, 14, Math.Max(0, ActualWidth - 28), Math.Max(0, ActualHeight - 28));
        dc.DrawRectangle(new SolidColorBrush(WithAlpha(surface, 0x66)), new Pen(new SolidColorBrush(border), 1), frameRect);
        DrawGrid(dc, frameRect);

        var motions = Motions?.Where(m => m.IsExecutable).ToList() ?? new List<GcodeMotionCommand>();
        if (motions.Count == 0)
            return;

        var worldWidth = Math.Max(MachineWidth, motions.Max(m => (double)Math.Max(m.StartX, m.EndX)));
        var worldHeight = Math.Max(MachineHeight, motions.Max(m => (double)Math.Max(m.StartY, m.EndY)));
        worldWidth = Math.Max(worldWidth, 1d);
        worldHeight = Math.Max(worldHeight, 1d);

        var scale = Math.Min(frameRect.Width / worldWidth, frameRect.Height / worldHeight);
        var offsetX = frameRect.Left + (frameRect.Width - (worldWidth * scale)) / 2d;
        var offsetY = frameRect.Top + (frameRect.Height - (worldHeight * scale)) / 2d;

        Point Map(decimal x, decimal y) => new(offsetX + ((double)x * scale), offsetY + ((double)y * scale));

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
            var start = Map(motion.StartX, motion.StartY);
            var end = Map(motion.EndX, motion.EndY);

            Pen pen;
            if (CurrentSegmentIndex >= 0 && i < CurrentSegmentIndex)
            {
                pen = motion.IsRapidMove ? completedRapidPen : completedCutPen;
            }
            else if (i == CurrentSegmentIndex)
            {
                pen = currentPen;
            }
            else
            {
                pen = motion.IsRapidMove ? remainingRapidPen : remainingCutPen;
            }

            dc.DrawLine(motion.IsRapidMove ? rapidPen : cutPen, start, end);
            dc.DrawLine(pen, start, end);

            if (i == CurrentSegmentIndex && SegmentProgress > 0d && SegmentProgress < 1d)
            {
                var currentPoint = new Point(
                    start.X + ((end.X - start.X) * SegmentProgress),
                    start.Y + ((end.Y - start.Y) * SegmentProgress));
                dc.DrawLine(currentPen, start, currentPoint);
            }
        }

        var toolPoint = Map((decimal)ToolX, (decimal)ToolY);
        var toolFill = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
        var toolStroke = new Pen(new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), 1.5);
        dc.DrawEllipse(toolFill, toolStroke, toolPoint, 5, 5);
        dc.DrawLine(toolStroke, new Point(toolPoint.X - 8, toolPoint.Y), new Point(toolPoint.X + 8, toolPoint.Y));
        dc.DrawLine(toolStroke, new Point(toolPoint.X, toolPoint.Y - 8), new Point(toolPoint.X, toolPoint.Y + 8));
    }

    private void DrawGrid(DrawingContext dc, Rect frameRect)
    {
        var gridPen = new Pen(new SolidColorBrush(WithAlpha(ThemeColor("BorderColor", Color.FromRgb(0xBD, 0xC8, 0xD4)), 0x66)), 0.7);
        const int divisions = 10;
        for (var i = 1; i < divisions; i++)
        {
            var x = frameRect.Left + (frameRect.Width / divisions) * i;
            var y = frameRect.Top + (frameRect.Height / divisions) * i;
            dc.DrawLine(gridPen, new Point(x, frameRect.Top), new Point(x, frameRect.Bottom));
            dc.DrawLine(gridPen, new Point(frameRect.Left, y), new Point(frameRect.Right, y));
        }
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
}

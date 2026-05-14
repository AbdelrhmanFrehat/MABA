using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MabaControlCenter.Models;
using MabaControlCenter.Services;

namespace MabaControlCenter.Controls;

public class CncMachineViewportControl : FrameworkElement
{
    public static readonly DependencyProperty MotionsProperty =
        DependencyProperty.Register(
            nameof(Motions),
            typeof(IEnumerable<GcodeMotionCommand>),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty CurrentMotionIndexProperty =
        DependencyProperty.Register(
            nameof(CurrentMotionIndex),
            typeof(int),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMinXProperty =
        DependencyProperty.Register(
            nameof(MachineMinX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMaxXProperty =
        DependencyProperty.Register(
            nameof(MachineMaxX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMinYProperty =
        DependencyProperty.Register(
            nameof(MachineMinY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMaxYProperty =
        DependencyProperty.Register(
            nameof(MachineMaxY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMinZProperty =
        DependencyProperty.Register(
            nameof(MachineMinZ),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineMaxZProperty =
        DependencyProperty.Register(
            nameof(MachineMaxZ),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(80d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty CurrentXProperty =
        DependencyProperty.Register(
            nameof(CurrentX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty CurrentYProperty =
        DependencyProperty.Register(
            nameof(CurrentY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty CurrentZProperty =
        DependencyProperty.Register(
            nameof(CurrentZ),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty WorkOffsetXProperty =
        DependencyProperty.Register(
            nameof(WorkOffsetX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty WorkOffsetYProperty =
        DependencyProperty.Register(
            nameof(WorkOffsetY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty WorkOffsetZProperty =
        DependencyProperty.Register(
            nameof(WorkOffsetZ),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty MachineStateProperty =
        DependencyProperty.Register(
            nameof(MachineState),
            typeof(CncMachineState),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(CncMachineState.Disconnected, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty ExecutionStateProperty =
        DependencyProperty.Register(
            nameof(ExecutionState),
            typeof(CncExecutionState),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(CncExecutionState.Idle, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty HasFrameBoundsProperty =
        DependencyProperty.Register(
            nameof(HasFrameBounds),
            typeof(bool),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty FrameMinXProperty =
        DependencyProperty.Register(
            nameof(FrameMinX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty FrameMaxXProperty =
        DependencyProperty.Register(
            nameof(FrameMaxX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty FrameMinYProperty =
        DependencyProperty.Register(
            nameof(FrameMinY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty FrameMaxYProperty =
        DependencyProperty.Register(
            nameof(FrameMaxY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty IsFramePreviewActiveProperty =
        DependencyProperty.Register(
            nameof(IsFramePreviewActive),
            typeof(bool),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty PreviewToolXProperty =
        DependencyProperty.Register(
            nameof(PreviewToolX),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty PreviewToolYProperty =
        DependencyProperty.Register(
            nameof(PreviewToolY),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty PreviewCurrentSegmentIndexProperty =
        DependencyProperty.Register(
            nameof(PreviewCurrentSegmentIndex),
            typeof(int),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty PreviewSegmentProgressProperty =
        DependencyProperty.Register(
            nameof(PreviewSegmentProgress),
            typeof(double),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    public static readonly DependencyProperty UsePreviewPlaybackProperty =
        DependencyProperty.Register(
            nameof(UsePreviewPlayback),
            typeof(bool),
            typeof(CncMachineViewportControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualInputChanged));

    private readonly DispatcherTimer _animationTimer;
    private double _animatedX;
    private double _animatedY;
    private double _animatedZ;
    private bool _isAnimationInitialized;

    public CncMachineViewportControl()
    {
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _animationTimer.Tick += (_, _) => AnimateTowardTarget();

        Loaded += (_, _) => _animationTimer.Start();
        Unloaded += (_, _) => _animationTimer.Stop();
    }

    public IEnumerable<GcodeMotionCommand>? Motions
    {
        get => (IEnumerable<GcodeMotionCommand>?)GetValue(MotionsProperty);
        set => SetValue(MotionsProperty, value);
    }

    public int CurrentMotionIndex
    {
        get => (int)GetValue(CurrentMotionIndexProperty);
        set => SetValue(CurrentMotionIndexProperty, value);
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

    public double MachineMinZ
    {
        get => (double)GetValue(MachineMinZProperty);
        set => SetValue(MachineMinZProperty, value);
    }

    public double MachineMaxZ
    {
        get => (double)GetValue(MachineMaxZProperty);
        set => SetValue(MachineMaxZProperty, value);
    }

    public double CurrentX
    {
        get => (double)GetValue(CurrentXProperty);
        set => SetValue(CurrentXProperty, value);
    }

    public double CurrentY
    {
        get => (double)GetValue(CurrentYProperty);
        set => SetValue(CurrentYProperty, value);
    }

    public double CurrentZ
    {
        get => (double)GetValue(CurrentZProperty);
        set => SetValue(CurrentZProperty, value);
    }

    public double WorkOffsetX
    {
        get => (double)GetValue(WorkOffsetXProperty);
        set => SetValue(WorkOffsetXProperty, value);
    }

    public double WorkOffsetY
    {
        get => (double)GetValue(WorkOffsetYProperty);
        set => SetValue(WorkOffsetYProperty, value);
    }

    public double WorkOffsetZ
    {
        get => (double)GetValue(WorkOffsetZProperty);
        set => SetValue(WorkOffsetZProperty, value);
    }

    public CncMachineState MachineState
    {
        get => (CncMachineState)GetValue(MachineStateProperty);
        set => SetValue(MachineStateProperty, value);
    }

    public CncExecutionState ExecutionState
    {
        get => (CncExecutionState)GetValue(ExecutionStateProperty);
        set => SetValue(ExecutionStateProperty, value);
    }

    public bool HasFrameBounds
    {
        get => (bool)GetValue(HasFrameBoundsProperty);
        set => SetValue(HasFrameBoundsProperty, value);
    }

    public double FrameMinX
    {
        get => (double)GetValue(FrameMinXProperty);
        set => SetValue(FrameMinXProperty, value);
    }

    public double FrameMaxX
    {
        get => (double)GetValue(FrameMaxXProperty);
        set => SetValue(FrameMaxXProperty, value);
    }

    public double FrameMinY
    {
        get => (double)GetValue(FrameMinYProperty);
        set => SetValue(FrameMinYProperty, value);
    }

    public double FrameMaxY
    {
        get => (double)GetValue(FrameMaxYProperty);
        set => SetValue(FrameMaxYProperty, value);
    }

    public bool IsFramePreviewActive
    {
        get => (bool)GetValue(IsFramePreviewActiveProperty);
        set => SetValue(IsFramePreviewActiveProperty, value);
    }

    public double PreviewToolX
    {
        get => (double)GetValue(PreviewToolXProperty);
        set => SetValue(PreviewToolXProperty, value);
    }

    public double PreviewToolY
    {
        get => (double)GetValue(PreviewToolYProperty);
        set => SetValue(PreviewToolYProperty, value);
    }

    public int PreviewCurrentSegmentIndex
    {
        get => (int)GetValue(PreviewCurrentSegmentIndexProperty);
        set => SetValue(PreviewCurrentSegmentIndexProperty, value);
    }

    public double PreviewSegmentProgress
    {
        get => (double)GetValue(PreviewSegmentProgressProperty);
        set => SetValue(PreviewSegmentProgressProperty, value);
    }

    public bool UsePreviewPlayback
    {
        get => (bool)GetValue(UsePreviewPlaybackProperty);
        set => SetValue(UsePreviewPlaybackProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var fullRect = new Rect(0, 0, ActualWidth, ActualHeight);
        dc.DrawRectangle(new SolidColorBrush(ThemeColor("BackgroundLight", Color.FromRgb(0x15, 0x1E, 0x2C))), null, fullRect);

        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        EnsureAnimationInitialized();

        var frame = new Rect(12, 12, Math.Max(0, ActualWidth - 24), Math.Max(0, ActualHeight - 24));
        var transform = new CncViewportTransform(frame, MachineMinX, MachineMaxX, MachineMinY, MachineMaxY);
        var motions = (Motions ?? Enumerable.Empty<GcodeMotionCommand>()).Where(m => m.IsExecutable).ToList();

        DrawViewportChrome(dc, transform);
        DrawPaths(dc, transform, motions);
        DrawFrameOverlay(dc, transform);
        DrawWorkZero(dc, transform);
        DrawToolMarker(dc, transform);
        DrawOverlayText(dc, transform, motions.Count);
    }

    private void DrawViewportChrome(DrawingContext dc, CncViewportTransform transform)
    {
        var bedFill = new SolidColorBrush(WithAlpha(ThemeColor("BackgroundMedium", Color.FromRgb(0x0D, 0x15, 0x20)), 0xC8));
        var bedPen = new Pen(new SolidColorBrush(ThemeColor("BorderColor", Color.FromRgb(0x51, 0x63, 0x78))), 1.1);
        dc.DrawRectangle(bedFill, bedPen, transform.BedRect);

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

    private void DrawPaths(DrawingContext dc, CncViewportTransform transform, IReadOnlyList<GcodeMotionCommand> motions)
    {
        if (motions.Count == 0)
            return;

        var completedCutPen = new Pen(new SolidColorBrush(Color.FromRgb(0x94, 0xFF, 0xC8)), 2.4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var completedRapidPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0xAA)), 1.4) { DashStyle = DashStyles.Dash, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var remainingCutPen = new Pen(new SolidColorBrush(Color.FromArgb(0x77, 0x35, 0xC0, 0x86)), 1.45) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var remainingRapidPen = new Pen(new SolidColorBrush(Color.FromArgb(0x99, 0xE2, 0x6C, 0x6C)), 0.9) { DashStyle = DashStyles.Dot, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var currentPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x49)), 2.8) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        var activeIndex = UsePreviewPlayback
            ? PreviewCurrentSegmentIndex
            : ExecutionState == CncExecutionState.Completed
                ? -1
                : CurrentMotionIndex;

        var progress = UsePreviewPlayback ? PreviewSegmentProgress : 1d;

        for (var i = 0; i < motions.Count; i++)
        {
            var motion = motions[i];

            Pen pen;
            if (activeIndex >= 0 && i < activeIndex)
            {
                pen = motion.IsRapidMove ? completedRapidPen : completedCutPen;
            }
            else if (i == activeIndex)
            {
                pen = currentPen;
            }
            else
            {
                pen = motion.IsRapidMove ? remainingRapidPen : remainingCutPen;
            }

            DrawMotion(dc, motion, transform, pen);

            if (i == activeIndex && progress > 0d && progress < 1d)
            {
                DrawPartialMotion(dc, motion, transform, currentPen, progress);
            }
        }
    }

    private void DrawFrameOverlay(DrawingContext dc, CncViewportTransform transform)
    {
        if (!HasFrameBounds || FrameMaxX <= FrameMinX || FrameMaxY <= FrameMinY)
            return;

        var p1 = transform.Map(FrameMinX, FrameMinY);
        var p2 = transform.Map(FrameMaxX, FrameMinY);
        var p3 = transform.Map(FrameMaxX, FrameMaxY);
        var p4 = transform.Map(FrameMinX, FrameMaxY);
        var framePen = new Pen(new SolidColorBrush(Color.FromRgb(0x5B, 0xE0, 0xFF)), 1.8) { DashStyle = DashStyles.Dash };

        dc.DrawLine(framePen, p1, p2);
        dc.DrawLine(framePen, p2, p3);
        dc.DrawLine(framePen, p3, p4);
        dc.DrawLine(framePen, p4, p1);

        DrawLabel(dc, "FRAME", new Point(p1.X + 8, p1.Y - 18), Color.FromRgb(0x5B, 0xE0, 0xFF), 11, FontWeights.SemiBold);

        if (!IsFramePreviewActive)
            return;

        var frameTool = transform.Map(PreviewToolX + WorkOffsetX, PreviewToolY + WorkOffsetY);
        var toolPen = new Pen(new SolidColorBrush(Color.FromRgb(0x5B, 0xE0, 0xFF)), 1.4);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x40, 0x5B, 0xE0, 0xFF)), toolPen, frameTool, 6, 6);
        dc.DrawLine(toolPen, new Point(frameTool.X - 7, frameTool.Y), new Point(frameTool.X + 7, frameTool.Y));
        dc.DrawLine(toolPen, new Point(frameTool.X, frameTool.Y - 7), new Point(frameTool.X, frameTool.Y + 7));
    }

    private void DrawWorkZero(DrawingContext dc, CncViewportTransform transform)
    {
        if (WorkOffsetX < MachineMinX || WorkOffsetX > MachineMaxX || WorkOffsetY < MachineMinY || WorkOffsetY > MachineMaxY)
            return;

        var originPoint = transform.Map(WorkOffsetX, WorkOffsetY);
        var crossPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFB, 0x71, 0x71)), 1.4);
        dc.DrawLine(crossPen, new Point(originPoint.X - 7, originPoint.Y), new Point(originPoint.X + 7, originPoint.Y));
        dc.DrawLine(crossPen, new Point(originPoint.X, originPoint.Y - 7), new Point(originPoint.X, originPoint.Y + 7));
        DrawLabel(dc, "WORK ZERO", new Point(originPoint.X + 10, originPoint.Y - 18), Color.FromRgb(0xFB, 0x71, 0x71), 11, FontWeights.SemiBold);
    }

    private void DrawToolMarker(DrawingContext dc, CncViewportTransform transform)
    {
        var displayX = UsePreviewPlayback ? PreviewToolX + WorkOffsetX : _animatedX;
        var displayY = UsePreviewPlayback ? PreviewToolY + WorkOffsetY : _animatedY;
        var point = transform.Map(displayX, displayY);
        var toolColor = GetStateColor();
        var toolPen = new Pen(new SolidColorBrush(toolColor), 1.8);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x40, toolColor.R, toolColor.G, toolColor.B)), toolPen, point, 7, 7);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC)), toolPen, point, 4.5, 4.5);
        dc.DrawLine(toolPen, new Point(point.X - 8, point.Y), new Point(point.X + 8, point.Y));
        dc.DrawLine(toolPen, new Point(point.X, point.Y - 8), new Point(point.X, point.Y + 8));
    }

    private void DrawOverlayText(DrawingContext dc, CncViewportTransform transform, int motionCount)
    {
        DrawLabel(
            dc,
            $"{FormatMachineState(MachineState)} / {FormatExecutionState(ExecutionState)}",
            new Point(transform.BedRect.Right - 166, transform.BedRect.Top - 20),
            GetStateColor(),
            12,
            FontWeights.SemiBold);

        var displayX = UsePreviewPlayback ? PreviewToolX + WorkOffsetX : _animatedX;
        var displayY = UsePreviewPlayback ? PreviewToolY + WorkOffsetY : _animatedY;
        DrawLabel(
            dc,
            $"Tool: X {displayX:0.###}  Y {displayY:0.###}  Z {_animatedZ:0.###} mm",
            new Point(transform.BedRect.Left, transform.BedRect.Bottom + 28),
            ThemeColor("TextPrimary", Color.FromRgb(0xE2, 0xE8, 0xF0)),
            12,
            FontWeights.Medium);

        if (motionCount == 0)
        {
            DrawLabel(
                dc,
                "No job loaded. Bed, grid, origin, and tool position are shown using live machine dimensions.",
                new Point(transform.BedRect.Left, transform.BedRect.Top + 10),
                ThemeColor("TextSecondary", Color.FromRgb(0x94, 0xA3, 0xB8)),
                11,
                FontWeights.Medium);
        }
    }

    private void AnimateTowardTarget()
    {
        EnsureAnimationInitialized();

        var targetX = UsePreviewPlayback ? PreviewToolX + WorkOffsetX : CurrentX;
        var targetY = UsePreviewPlayback ? PreviewToolY + WorkOffsetY : CurrentY;
        var nextX = MoveToward(_animatedX, targetX);
        var nextY = MoveToward(_animatedY, targetY);
        var nextZ = MoveToward(_animatedZ, CurrentZ);

        if (Math.Abs(nextX - _animatedX) < 0.001 &&
            Math.Abs(nextY - _animatedY) < 0.001 &&
            Math.Abs(nextZ - _animatedZ) < 0.001)
        {
            _animatedX = targetX;
            _animatedY = targetY;
            _animatedZ = CurrentZ;
            InvalidateVisual();
            return;
        }

        _animatedX = nextX;
        _animatedY = nextY;
        _animatedZ = nextZ;
        InvalidateVisual();
    }

    private static double MoveToward(double current, double target)
    {
        var delta = target - current;
        if (Math.Abs(delta) < 0.001)
            return target;

        return current + (delta * 0.24);
    }

    private void EnsureAnimationInitialized()
    {
        if (_isAnimationInitialized)
            return;

        _animatedX = CurrentX;
        _animatedY = CurrentY;
        _animatedZ = CurrentZ;
        _isAnimationInitialized = true;
    }

    private static void OnVisualInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CncMachineViewportControl control)
            control.InvalidateVisual();
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

    private static void DrawArrowHead(DrawingContext dc, Pen pen, Point tip, Vector direction)
    {
        direction.Normalize();
        var left = new Vector(-direction.Y, direction.X);
        var back = -direction * 8d;
        var wing = left * 4d;
        dc.DrawLine(pen, tip, tip + back + wing);
        dc.DrawLine(pen, tip, tip + back - wing);
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

    private Color GetStateColor()
    {
        return MachineState switch
        {
            CncMachineState.Running => Color.FromRgb(0x38, 0xBD, 0xF8),
            CncMachineState.Completed => Color.FromRgb(0x34, 0xD3, 0x99),
            CncMachineState.Paused => Color.FromRgb(0xF5, 0x9E, 0x0B),
            CncMachineState.Warning => Color.FromRgb(0xFB, 0x92, 0x3C),
            CncMachineState.Alarm => Color.FromRgb(0xF9, 0x73, 0x16),
            CncMachineState.Error => Color.FromRgb(0xEF, 0x44, 0x44),
            CncMachineState.Disconnected => Color.FromRgb(0x94, 0xA3, 0xB8),
            CncMachineState.Homing => Color.FromRgb(0xA7, 0x8B, 0xFA),
            _ => Color.FromRgb(0xE2, 0xE8, 0xF0)
        };
    }

    private static string FormatMachineState(CncMachineState state)
    {
        return state switch
        {
            CncMachineState.Disconnected => "Disconnected",
            CncMachineState.Idle => "Idle",
            CncMachineState.Homing => "Homing",
            CncMachineState.Running => "Running",
            CncMachineState.Paused => "Paused",
            CncMachineState.Stopped => "Stopped",
            CncMachineState.Completed => "Completed",
            CncMachineState.Warning => "Warning",
            CncMachineState.Alarm => "Alarm",
            CncMachineState.Error => "Error",
            _ => state.ToString()
        };
    }

    private static string FormatExecutionState(CncExecutionState state)
    {
        return state switch
        {
            CncExecutionState.Idle => "Idle",
            CncExecutionState.JobLoaded => "Job Loaded",
            CncExecutionState.PreflightChecking => "Preflight",
            CncExecutionState.ReadyToRun => "Ready",
            CncExecutionState.Running => "Running",
            CncExecutionState.Paused => "Paused",
            CncExecutionState.Stopping => "Stopping",
            CncExecutionState.Stopped => "Stopped",
            CncExecutionState.Alarmed => "Alarmed",
            CncExecutionState.Error => "Error",
            CncExecutionState.Failed => "Failed",
            CncExecutionState.Completed => "Completed",
            _ => state.ToString()
        };
    }
}

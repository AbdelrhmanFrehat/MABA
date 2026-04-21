using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MabaControlCenter.Models;

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

        var frame = new Rect(14, 14, Math.Max(0, ActualWidth - 28), Math.Max(0, ActualHeight - 28));
        if (frame.Width <= 0 || frame.Height <= 0)
            return;

        var worldWidth = Math.Max(1d, MachineMaxX - MachineMinX);
        var worldHeight = Math.Max(1d, MachineMaxY - MachineMinY);
        var worldDepth = Math.Max(1d, MachineMaxZ - MachineMinZ);

        var surfaceWidth = frame.Width * 0.68;
        var surfaceHeight = frame.Height * 0.50;
        var skew = Math.Min(frame.Width, frame.Height) * 0.14;
        var topLeft = new Point(frame.Left + 70, frame.Top + 56);
        var topRight = new Point(topLeft.X + surfaceWidth, topLeft.Y);
        var bottomLeft = new Point(topLeft.X - skew, topLeft.Y + surfaceHeight);
        var bottomRight = new Point(topRight.X - skew, topRight.Y + surfaceHeight);
        var thickness = Math.Max(16, frame.Height * 0.05);

        var sideRightBottom = new Point(bottomRight.X, bottomRight.Y + thickness);
        var sideLeftBottom = new Point(bottomLeft.X, bottomLeft.Y + thickness);
        var frontLeftBottom = new Point(bottomLeft.X, bottomLeft.Y + thickness);
        var frontRightBottom = new Point(bottomRight.X, bottomRight.Y + thickness);

        DrawBed(dc, topLeft, topRight, bottomLeft, bottomRight, frontLeftBottom, frontRightBottom, sideRightBottom);
        DrawGrid(dc, worldWidth, worldHeight, topLeft, topRight, bottomLeft);

        var motions = (Motions ?? Enumerable.Empty<GcodeMotionCommand>()).Where(m => m.IsExecutable).ToList();
        DrawPaths(dc, motions, worldWidth, worldHeight, worldDepth, topLeft, topRight, bottomLeft);
        DrawFrameOverlay(dc, worldWidth, worldHeight, topLeft, topRight, bottomLeft);
        DrawWorkZero(dc, worldWidth, worldHeight, topLeft, topRight, bottomLeft);
        DrawGantry(dc, worldWidth, worldHeight, worldDepth, topLeft, topRight, bottomLeft);
        DrawOverlays(dc, frame, worldWidth, worldHeight);
    }

    private void DrawBed(
        DrawingContext dc,
        Point topLeft,
        Point topRight,
        Point bottomLeft,
        Point bottomRight,
        Point frontLeftBottom,
        Point frontRightBottom,
        Point sideRightBottom)
    {
        var topSurface = new StreamGeometry();
        using (var ctx = topSurface.Open())
        {
            ctx.BeginFigure(topLeft, true, true);
            ctx.LineTo(topRight, true, false);
            ctx.LineTo(bottomRight, true, false);
            ctx.LineTo(bottomLeft, true, false);
        }
        topSurface.Freeze();

        var frontFace = new StreamGeometry();
        using (var ctx = frontFace.Open())
        {
            ctx.BeginFigure(bottomLeft, true, true);
            ctx.LineTo(bottomRight, true, false);
            ctx.LineTo(frontRightBottom, true, false);
            ctx.LineTo(frontLeftBottom, true, false);
        }
        frontFace.Freeze();

        var sideFace = new StreamGeometry();
        using (var ctx = sideFace.Open())
        {
            ctx.BeginFigure(topRight, true, true);
            ctx.LineTo(bottomRight, true, false);
            ctx.LineTo(sideRightBottom, true, false);
            ctx.LineTo(new Point(topRight.X, topRight.Y + (sideRightBottom.Y - bottomRight.Y)), true, false);
        }
        sideFace.Freeze();

        var backgroundLight = ThemeColor("BackgroundLight", Color.FromRgb(0x2A, 0x35, 0x46));
        var backgroundMedium = ThemeColor("BackgroundMedium", Color.FromRgb(0x1C, 0x25, 0x33));
        var backgroundDark = ThemeColor("BackgroundDark", Color.FromRgb(0x11, 0x18, 0x24));
        var border = ThemeColor("BorderColor", Color.FromRgb(0x65, 0x74, 0x84));
        var framePen = new Pen(new SolidColorBrush(border), 1.2);
        dc.DrawGeometry(new LinearGradientBrush(backgroundLight, backgroundMedium, 75), framePen, topSurface);
        dc.DrawGeometry(new SolidColorBrush(WithAlpha(backgroundMedium, 0xEE)), null, frontFace);
        dc.DrawGeometry(new SolidColorBrush(WithAlpha(backgroundDark, 0xEE)), null, sideFace);

        dc.DrawLine(new Pen(new SolidColorBrush(WithAlpha(ThemeColor("TextPrimary", Color.FromRgb(0xF8, 0xFA, 0xFC)), 0x40)), 2), topLeft, topRight);
        dc.DrawLine(new Pen(new SolidColorBrush(WithAlpha(ThemeColor("TextPrimary", Color.FromRgb(0xF8, 0xFA, 0xFC)), 0x28)), 1), topLeft, bottomLeft);
    }

    private void DrawGrid(DrawingContext dc, double worldWidth, double worldHeight, Point topLeft, Point topRight, Point bottomLeft)
    {
        var gridPen = new Pen(new SolidColorBrush(WithAlpha(ThemeColor("BorderColor", Color.FromRgb(0x94, 0xA3, 0xB8)), 0x80)), 0.8);

        for (var i = 1; i < 8; i++)
        {
            var ratio = i / 8d;
            var xStart = Lerp(ProjectNormalized(topLeft, topRight, bottomLeft, 0, 0), ProjectNormalized(topLeft, topRight, bottomLeft, 1, 0), ratio);
            var xEnd = Lerp(ProjectNormalized(topLeft, topRight, bottomLeft, 0, 1), ProjectNormalized(topLeft, topRight, bottomLeft, 1, 1), ratio);
            dc.DrawLine(gridPen, xStart, xEnd);
        }

        for (var i = 1; i < 7; i++)
        {
            var ratio = i / 7d;
            var yStart = Lerp(ProjectNormalized(topLeft, topRight, bottomLeft, 0, 0), ProjectNormalized(topLeft, topRight, bottomLeft, 0, 1), ratio);
            var yEnd = Lerp(ProjectNormalized(topLeft, topRight, bottomLeft, 1, 0), ProjectNormalized(topLeft, topRight, bottomLeft, 1, 1), ratio);
            dc.DrawLine(gridPen, yStart, yEnd);
        }
    }

    private void DrawPaths(
        DrawingContext dc,
        IReadOnlyList<GcodeMotionCommand> motions,
        double worldWidth,
        double worldHeight,
        double worldDepth,
        Point topLeft,
        Point topRight,
        Point bottomLeft)
    {
        if (motions.Count == 0)
            return;

        var completedPen = new Pen(new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)), 2.1);
        var remainingPen = new Pen(new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0xF9)), 1.4);
        var currentPen = new Pen(new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)), 3.0);
        var shadowPen = new Pen(new SolidColorBrush(Color.FromArgb(0x35, 0x00, 0x00, 0x00)), 3.6);

        var activeIndex = UsePreviewPlayback
            ? PreviewCurrentSegmentIndex
            : ExecutionState == CncExecutionState.Completed
                ? -1
                : CurrentMotionIndex;

        var activeProgress = UsePreviewPlayback ? PreviewSegmentProgress : 0d;

        for (var i = 0; i < motions.Count; i++)
        {
            var motion = motions[i];
            var start = ProjectMotion(
                topLeft,
                topRight,
                bottomLeft,
                worldWidth,
                worldHeight,
                worldDepth,
                (double)motion.StartX + WorkOffsetX,
                (double)motion.StartY + WorkOffsetY,
                (double)motion.StartZ + WorkOffsetZ);
            var end = ProjectMotion(
                topLeft,
                topRight,
                bottomLeft,
                worldWidth,
                worldHeight,
                worldDepth,
                (double)motion.EndX + WorkOffsetX,
                (double)motion.EndY + WorkOffsetY,
                (double)motion.EndZ + WorkOffsetZ);

            var pen = ExecutionState == CncExecutionState.Completed || i < activeIndex
                ? completedPen
                : i == activeIndex
                    ? currentPen
                    : remainingPen;

            dc.DrawLine(shadowPen, new Point(start.X + 1.5, start.Y + 1.5), new Point(end.X + 1.5, end.Y + 1.5));
            dc.DrawLine(pen, start, end);

            if (i == activeIndex)
            {
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x55, 0xF5, 0x9E, 0x0B)), null, end, 7, 7);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7)), new Pen(new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)), 1.4), end, 4.2, 4.2);

                if (UsePreviewPlayback && activeProgress > 0d && activeProgress < 1d)
                {
                    var partialEnd = new Point(
                        start.X + ((end.X - start.X) * activeProgress),
                        start.Y + ((end.Y - start.Y) * activeProgress));
                    dc.DrawLine(currentPen, start, partialEnd);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x55, 0xF5, 0x9E, 0x0B)), null, partialEnd, 7, 7);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7)), new Pen(new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)), 1.4), partialEnd, 4.2, 4.2);
                }
            }
        }
    }

    private void DrawWorkZero(DrawingContext dc, double worldWidth, double worldHeight, Point topLeft, Point topRight, Point bottomLeft)
    {
        if (WorkOffsetX < MachineMinX || WorkOffsetX > MachineMaxX || WorkOffsetY < MachineMinY || WorkOffsetY > MachineMaxY)
            return;

        var originPoint = ProjectMachine(topLeft, topRight, bottomLeft, WorkOffsetX - MachineMinX, WorkOffsetY - MachineMinY);
        var crossPen = new Pen(new SolidColorBrush(Color.FromRgb(0xFB, 0x71, 0x71)), 1.4);
        dc.DrawLine(crossPen, new Point(originPoint.X - 7, originPoint.Y), new Point(originPoint.X + 7, originPoint.Y));
        dc.DrawLine(crossPen, new Point(originPoint.X, originPoint.Y - 7), new Point(originPoint.X, originPoint.Y + 7));

        DrawLabel(dc, "WORK ZERO", new Point(originPoint.X + 10, originPoint.Y - 18), Color.FromRgb(0xFB, 0x71, 0x71), 11, FontWeights.SemiBold);
    }

    private void DrawFrameOverlay(DrawingContext dc, double worldWidth, double worldHeight, Point topLeft, Point topRight, Point bottomLeft)
    {
        if (!HasFrameBounds || FrameMaxX <= FrameMinX || FrameMaxY <= FrameMinY)
            return;

        var p1 = ProjectMachine(topLeft, topRight, bottomLeft, FrameMinX - MachineMinX, FrameMinY - MachineMinY);
        var p2 = ProjectMachine(topLeft, topRight, bottomLeft, FrameMaxX - MachineMinX, FrameMinY - MachineMinY);
        var p3 = ProjectMachine(topLeft, topRight, bottomLeft, FrameMaxX - MachineMinX, FrameMaxY - MachineMinY);
        var p4 = ProjectMachine(topLeft, topRight, bottomLeft, FrameMinX - MachineMinX, FrameMaxY - MachineMinY);

        var framePen = new Pen(new SolidColorBrush(Color.FromRgb(0x5B, 0xE0, 0xFF)), 1.8) { DashStyle = DashStyles.Dash };
        dc.DrawLine(framePen, p1, p2);
        dc.DrawLine(framePen, p2, p3);
        dc.DrawLine(framePen, p3, p4);
        dc.DrawLine(framePen, p4, p1);

        DrawLabel(dc, "FRAME", new Point(p1.X + 10, p1.Y - 18), Color.FromRgb(0x5B, 0xE0, 0xFF), 11, FontWeights.SemiBold);

        if (!IsFramePreviewActive)
            return;

        var frameTool = ProjectMachine(topLeft, topRight, bottomLeft, PreviewToolX - MachineMinX, PreviewToolY - MachineMinY);
        var toolPen = new Pen(new SolidColorBrush(Color.FromRgb(0x5B, 0xE0, 0xFF)), 1.4);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x40, 0x5B, 0xE0, 0xFF)), toolPen, frameTool, 6, 6);
        dc.DrawLine(toolPen, new Point(frameTool.X - 7, frameTool.Y), new Point(frameTool.X + 7, frameTool.Y));
        dc.DrawLine(toolPen, new Point(frameTool.X, frameTool.Y - 7), new Point(frameTool.X, frameTool.Y + 7));
    }

    private void DrawGantry(DrawingContext dc, double worldWidth, double worldHeight, double worldDepth, Point topLeft, Point topRight, Point bottomLeft)
    {
        var clampedX = Clamp(_animatedX, MachineMinX, MachineMaxX);
        var clampedY = Clamp(_animatedY, MachineMinY, MachineMaxY);
        var clampedZ = Clamp(_animatedZ, MachineMinZ, MachineMaxZ);

        var localX = clampedX - MachineMinX;
        var localY = clampedY - MachineMinY;

        var leftBase = ProjectMachine(topLeft, topRight, bottomLeft, 0, localY);
        var rightBase = ProjectMachine(topLeft, topRight, bottomLeft, worldWidth, localY);
        var gantryLift = Math.Max(22, ActualHeight * 0.08);
        var leftTop = new Point(leftBase.X, leftBase.Y - gantryLift);
        var rightTop = new Point(rightBase.X, rightBase.Y - gantryLift);

        var carriageT = worldWidth <= 0 ? 0 : localX / worldWidth;
        var carriage = Lerp(leftTop, rightTop, carriageT);
        var toolTip = ProjectMotion(topLeft, topRight, bottomLeft, worldWidth, worldHeight, worldDepth, clampedX, clampedY, clampedZ);
        var toolColor = GetStateColor();

        var railPen = new Pen(new SolidColorBrush(Color.FromRgb(0xB8, 0xC5, 0xD2)), 3);
        var gantryPen = new Pen(new SolidColorBrush(Color.FromRgb(0xD4, 0xDE, 0xE8)), 4);
        var toolPen = new Pen(new SolidColorBrush(toolColor), 2.4);

        dc.DrawLine(railPen, leftBase, leftTop);
        dc.DrawLine(railPen, rightBase, rightTop);
        dc.DrawLine(gantryPen, leftTop, rightTop);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)), new Pen(new SolidColorBrush(Color.FromRgb(0x7C, 0x8A, 0x99)), 1), carriage, 7, 7);
        dc.DrawLine(toolPen, carriage, toolTip);
        dc.DrawEllipse(new SolidColorBrush(toolColor), new Pen(new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC)), 1.2), toolTip, 5.5, 5.5);
        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(0x30, toolColor.R, toolColor.G, toolColor.B)), null, toolTip, 10, 10);
    }

    private void DrawOverlays(DrawingContext dc, Rect frame, double worldWidth, double worldHeight)
    {
        var stateText = $"{FormatMachineState(MachineState)} / {FormatExecutionState(ExecutionState)}";
        var stateColor = GetStateColor();
        DrawLabel(dc, stateText, new Point(frame.Right - 174, frame.Top + 4), stateColor, 12, FontWeights.SemiBold);

        var displayX = UsePreviewPlayback ? PreviewToolX : _animatedX;
        var displayY = UsePreviewPlayback ? PreviewToolY : _animatedY;
        DrawLabel(dc, $"Tool: X {displayX:0.##}  Y {displayY:0.##}  Z {_animatedZ:0.##} mm", new Point(frame.Left + 8, frame.Bottom - 28), ThemeColor("TextPrimary", Color.FromRgb(0xE2, 0xE8, 0xF0)), 12, FontWeights.Medium);
        DrawLabel(dc, $"Envelope: {worldWidth:0.#} x {worldHeight:0.#} mm", new Point(frame.Left + 8, frame.Top + 4), ThemeColor("TextSecondary", Color.FromRgb(0x94, 0xA3, 0xB8)), 11, FontWeights.Medium);

        var legendY = frame.Bottom - 52;
        DrawLegendSwatch(dc, new Point(frame.Right - 238, legendY), Color.FromRgb(0x34, 0xD3, 0x99), "Executed");
        DrawLegendSwatch(dc, new Point(frame.Right - 156, legendY), Color.FromRgb(0xF5, 0x9E, 0x0B), "Active");
        DrawLegendSwatch(dc, new Point(frame.Right - 82, legendY), Color.FromRgb(0x60, 0x7D, 0xF9), "Remaining");

        if (MachineState == CncMachineState.Disconnected)
        {
            var overlay = new Rect(frame.Left + 20, frame.Top + 32, 150, 28);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(0x90, 0x7F, 0x1D, 0x1D)), new Pen(new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71)), 1), overlay, 8, 8);
            DrawLabel(dc, "LIVE CONTROL OFFLINE", new Point(overlay.Left + 10, overlay.Top + 6), Color.FromRgb(0xFE, 0xF2, 0xF2), 11, FontWeights.Bold);
        }
    }

    private void AnimateTowardTarget()
    {
        EnsureAnimationInitialized();

        var targetX = UsePreviewPlayback ? PreviewToolX : CurrentX;
        var targetY = UsePreviewPlayback ? PreviewToolY : CurrentY;
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
        if (d is not CncMachineViewportControl control)
            return;

        control.InvalidateVisual();
    }

    private Point ProjectMotion(Point topLeft, Point topRight, Point bottomLeft, double worldWidth, double worldHeight, double worldDepth, double machineX, double machineY, double machineZ)
    {
        var localX = machineX - MachineMinX;
        var localY = machineY - MachineMinY;
        var localZ = machineZ - MachineMinZ;
        var basePoint = ProjectMachine(topLeft, topRight, bottomLeft, localX, localY);
        var zRatio = worldDepth <= 0 ? 0 : (localZ / worldDepth);
        var verticalLift = zRatio * Math.Max(18, ActualHeight * 0.11);
        var depthShift = zRatio * Math.Max(8, ActualWidth * 0.02);
        return new Point(basePoint.X + depthShift, basePoint.Y - verticalLift);
    }

    private static Point ProjectNormalized(Point topLeft, Point topRight, Point bottomLeft, double x, double y)
    {
        var xVector = topRight - topLeft;
        var yVector = bottomLeft - topLeft;
        return new Point(topLeft.X + xVector.X * x + yVector.X * y, topLeft.Y + xVector.Y * x + yVector.Y * y);
    }

    private Point ProjectMachine(Point topLeft, Point topRight, Point bottomLeft, double localX, double localY)
    {
        var normalizedX = Normalize(localX, 0, Math.Max(1d, MachineMaxX - MachineMinX));
        var normalizedY = Normalize(localY, 0, Math.Max(1d, MachineMaxY - MachineMinY));
        return ProjectNormalized(topLeft, topRight, bottomLeft, normalizedX, normalizedY);
    }

    private static double Normalize(double value, double min, double max)
    {
        if (Math.Abs(max - min) < 0.0001)
            return 0;

        return (value - min) / (max - min);
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private static Point Lerp(Point start, Point end, double amount)
    {
        return new Point(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount));
    }

    private void DrawLegendSwatch(DrawingContext dc, Point position, Color color, string label)
    {
        dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(position.X, position.Y + 5, 16, 3));
        DrawLabel(dc, label, new Point(position.X + 22, position.Y), ThemeColor("TextPrimary", Color.FromRgb(0xCF, 0xD8, 0xE3)), 10.5, FontWeights.Medium);
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
            CncExecutionState.Running => "Running",
            CncExecutionState.Paused => "Paused",
            CncExecutionState.Stopped => "Stopped",
            CncExecutionState.Error => "Error",
            CncExecutionState.Completed => "Completed",
            _ => state.ToString()
        };
    }
}

using System.Collections.ObjectModel;

namespace MabaControlCenter.Models;

public enum ImageTraceMode
{
    Outline,
    Centerline
}

public enum VectorSegmentType
{
    Line,
    Arc
}

public sealed class ImageToolpathSettings
{
    public int Threshold { get; set; } = 140;
    public bool Invert { get; set; }
    public bool Grayscale { get; set; } = true;
    public bool Despeckle { get; set; } = true;
    public bool PreserveAspectRatio { get; set; } = true;
    public decimal TargetWidthMm { get; set; } = 50m;
    public decimal TargetHeightMm { get; set; } = 50m;
    public decimal SimplifyToleranceMm { get; set; } = 0.35m;
    public decimal CutDepthMm { get; set; } = -0.5m;
    public decimal SafeTravelZMm { get; set; } = 5m;
    public decimal CutFeedMmPerMinute { get; set; } = 300m;
    public decimal PlungeFeedMmPerMinute { get; set; } = 120m;
    public decimal RapidFeedMmPerMinute { get; set; } = 900m;
    public bool SpindleEnabled { get; set; } = true;
    public bool EnableZMoves { get; set; } = true;
    public ImageTraceMode TraceMode { get; set; } = ImageTraceMode.Outline;
}

public sealed class RasterTraceImage
{
    public bool[,] Mask { get; init; } = new bool[0, 0];
    public int WidthPixels { get; init; }
    public int HeightPixels { get; init; }
}

public sealed class VectorPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}

public sealed class VectorSegment
{
    public VectorSegmentType Type { get; set; } = VectorSegmentType.Line;
    public VectorPoint Start { get; set; } = new();
    public VectorPoint End { get; set; } = new();
    public VectorPoint? Center { get; set; }
    public bool Clockwise { get; set; }
}

public sealed class VectorPath
{
    public ObservableCollection<VectorPoint> Points { get; } = new();
    public ObservableCollection<VectorSegment> Segments { get; } = new();
    public bool Closed { get; set; }
    public bool IsCircleLike { get; set; }
    public double? CircleCenterX { get; set; }
    public double? CircleCenterY { get; set; }
    public double? CircleRadius { get; set; }
}

public sealed class GeneratedGcodeResult
{
    public string FileName { get; set; } = "image-toolpath.gcode";
    public string GcodeText { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool HasZMotion { get; set; }
    public ObservableCollection<string> Messages { get; } = new();
}

public sealed class ImageToolpathJob
{
    public string SourceImagePath { get; set; } = string.Empty;
    public ImageToolpathSettings Settings { get; set; } = new();
    public RasterTraceImage? PreprocessedImage { get; set; }
    public ObservableCollection<VectorPath> VectorPaths { get; } = new();
    public GeneratedGcodeResult GeneratedGcode { get; set; } = new();
}

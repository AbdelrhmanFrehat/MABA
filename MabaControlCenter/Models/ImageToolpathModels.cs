using System.Collections.ObjectModel;

namespace MabaControlCenter.Models;

public enum ImageTraceMode
{
    Outline,
    Centerline,
    TechnicalDrawing
}

public enum VectorSegmentType
{
    Line,
    Arc
}

public enum VectorPathKind
{
    Unknown,
    OuterContour,
    InnerHole,
    Island,
    OpenStroke
}

public enum ImageManufacturingMode
{
    OnLineTrace,
    ProfileOutside,
    ProfileInside
}

public enum ImagePreviewMarkerKind
{
    Start,
    End,
    Plunge,
    Retract,
    Order
}

public enum ImagePreviewMode
{
    Original,
    Thresholded,
    CleanedBinary,
    TracedVectors,
    FinalToolpath
}

public sealed class ImageToolpathSettings
{
    public int Threshold { get; set; } = 140;
    public bool UseAdaptiveThreshold { get; set; }
    public bool Invert { get; set; }
    public bool Grayscale { get; set; } = true;
    public bool Despeckle { get; set; } = true;
    public decimal SmoothingAmount { get; set; } = 0.2m;
    public decimal MinimumContourPerimeterMm { get; set; } = 1.5m;
    public decimal MinimumIslandSizeMm2 { get; set; } = 1.5m;
    public decimal MinimumFeatureSizeMm { get; set; } = 0.5m;
    public int MorphologyOpenIterations { get; set; }
    public int MorphologyCloseIterations { get; set; } = 1;
    public int DilationIterations { get; set; }
    public int ErosionIterations { get; set; }
    public bool PreserveAspectRatio { get; set; } = true;
    public decimal TargetWidthMm { get; set; } = 50m;
    public decimal TargetHeightMm { get; set; } = 50m;
    public decimal SimplifyToleranceMm { get; set; } = 0.35m;
    public decimal MinimumSegmentLengthMm { get; set; } = 0.15m;
    public decimal MinimumContourAreaMm2 { get; set; } = 1.5m;
    public decimal CloseGapToleranceMm { get; set; } = 0.4m;
    public decimal CutDepthMm { get; set; } = -0.5m;
    public decimal SafeTravelZMm { get; set; } = 5m;
    public decimal CutFeedMmPerMinute { get; set; } = 300m;
    public decimal PlungeFeedMmPerMinute { get; set; } = 120m;
    public decimal RapidFeedMmPerMinute { get; set; } = 900m;
    public bool SpindleEnabled { get; set; } = true;
    public bool EnableZMoves { get; set; } = true;
    public ImageTraceMode TraceMode { get; set; } = ImageTraceMode.Outline;
    public ImageManufacturingMode ManufacturingMode { get; set; } = ImageManufacturingMode.OnLineTrace;
}

public sealed class RasterTraceImage
{
    public bool[,] Mask { get; init; } = new bool[0, 0];
    public bool[,] ThresholdMask { get; init; } = new bool[0, 0];
    public bool[,] CleanedMask { get; init; } = new bool[0, 0];
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
    public int SourceIndex { get; set; }
    public ObservableCollection<VectorPoint> Points { get; } = new();
    public ObservableCollection<VectorSegment> Segments { get; } = new();
    public bool Closed { get; set; }
    public VectorPathKind Kind { get; set; } = VectorPathKind.Unknown;
    public int OrderIndex { get; set; }
    public int NestingDepth { get; set; }
    public int? ParentPathIndex { get; set; }
    public double SignedArea { get; set; }
    public double AbsoluteArea { get; set; }
    public double MinX { get; set; }
    public double MinY { get; set; }
    public double MaxX { get; set; }
    public double MaxY { get; set; }
    public double CentroidX { get; set; }
    public double CentroidY { get; set; }
    public bool IsCircleLike { get; set; }
    public double? CircleCenterX { get; set; }
    public double? CircleCenterY { get; set; }
    public double? CircleRadius { get; set; }
    public bool IsArcLike { get; set; }
    public double? ArcCenterX { get; set; }
    public double? ArcCenterY { get; set; }
    public double? ArcRadius { get; set; }
    public bool ArcClockwise { get; set; }
}

public sealed class GeneratedGcodeResult
{
    public string FileName { get; set; } = "image-toolpath.gcode";
    public string GcodeText { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool HasZMotion { get; set; }
    public decimal TotalCutDistanceMm { get; set; }
    public decimal TotalRapidDistanceMm { get; set; }
    public TimeSpan EstimatedJobTime { get; set; }
    public int GcodeLineCount { get; set; }
    public ObservableCollection<string> Messages { get; } = new();
}

public sealed class ImageToolpathDiagnostics
{
    public string TraceEngine { get; set; } = "Unknown";
    public int TracedPathCount { get; set; }
    public int RemovedPathCount { get; set; }
    public int JoinedPathCount { get; set; }
    public int ClosedContourCount { get; set; }
    public int OpenStrokeCount { get; set; }
    public int HoleCount { get; set; }
    public int LineFitCount { get; set; }
    public int ArcFitCount { get; set; }
    public int CircleFitCount { get; set; }
    public int SegmentReductionCount { get; set; }
    public int RemovedArtifactCount { get; set; }
    public int MergedSegmentCount { get; set; }
    public int SimplifiedPointCount { get; set; }
    public int RejectedTinyContourCount { get; set; }
    public decimal TotalCutDistanceMm { get; set; }
    public decimal TotalRapidDistanceMm { get; set; }
    public TimeSpan EstimatedJobTime { get; set; }
    public int GcodeLineCount { get; set; }
    public ObservableCollection<string> Warnings { get; } = new();
}

public sealed class ImagePreviewMarker
{
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public ImagePreviewMarkerKind Kind { get; set; } = ImagePreviewMarkerKind.Order;
}

public sealed class ImageToolpathPreview
{
    public string TracedGeometryData { get; set; } = string.Empty;
    public string CutGeometryData { get; set; } = string.Empty;
    public string RapidGeometryData { get; set; } = string.Empty;
    public string BoundingBoxGeometryData { get; set; } = string.Empty;
    public ObservableCollection<ImagePreviewMarker> Markers { get; } = new();
}

public sealed class VectorTraceResult
{
    public string TraceEngine { get; set; } = "Unknown";
    public IReadOnlyList<VectorPath> Paths { get; init; } = Array.Empty<VectorPath>();
    public int RemovedPathCount { get; init; }
    public int JoinedPathCount { get; init; }
    public int LineFitCount { get; init; }
    public int ArcFitCount { get; init; }
    public int CircleFitCount { get; init; }
    public int SegmentReductionCount { get; init; }
    public int RemovedArtifactCount { get; init; }
    public int MergedSegmentCount { get; init; }
    public int SimplifiedPointCount { get; init; }
    public int RejectedTinyContourCount { get; init; }
    public ObservableCollection<string> Warnings { get; } = new();
}

public sealed class ImageToolpathJob
{
    public string SourceImagePath { get; set; } = string.Empty;
    public ImageToolpathSettings Settings { get; set; } = new();
    public RasterTraceImage? PreprocessedImage { get; set; }
    public ObservableCollection<VectorPath> VectorPaths { get; } = new();
    public GeneratedGcodeResult GeneratedGcode { get; set; } = new();
    public ImageToolpathDiagnostics Diagnostics { get; set; } = new();
    public ImageToolpathPreview Preview { get; set; } = new();
}

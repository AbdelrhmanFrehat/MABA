using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IImagePreprocessorService
{
    RasterTraceImage Preprocess(string imagePath, ImageToolpathSettings settings);
}

public interface IVectorTraceService
{
    IReadOnlyList<VectorPath> Trace(RasterTraceImage image, ImageToolpathSettings settings);
}

public interface IToolpathFromVectorService
{
    GeneratedGcodeResult Generate(string imagePath, IReadOnlyList<VectorPath> vectorPaths, ImageToolpathSettings settings);
}

public interface IImageToolpathService
{
    ImageToolpathJob CreateJob(string imagePath, ImageToolpathSettings settings);
}

public sealed class ImagePreprocessorService : IImagePreprocessorService
{
    public RasterTraceImage Preprocess(string imagePath, ImageToolpathSettings settings)
    {
        var bitmap = BitmapFrame.Create(new Uri(imagePath, UriKind.Absolute));
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[height * stride];

        var converted = new FormatConvertedBitmap(bitmap, System.Windows.Media.PixelFormats.Bgra32, null, 0);
        converted.CopyPixels(pixels, stride, 0);

        var mask = new bool[height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * stride) + (x * 4);
                var b = pixels[index];
                var g = pixels[index + 1];
                var r = pixels[index + 2];
                var alpha = pixels[index + 3];

                var grayscale = settings.Grayscale
                    ? (int)Math.Round((0.299 * r) + (0.587 * g) + (0.114 * b))
                    : Math.Max(r, Math.Max(g, b));

                var isDark = alpha > 0 && grayscale <= settings.Threshold;
                mask[y, x] = settings.Invert ? !isDark : isDark;
            }
        }

        if (settings.Despeckle)
            mask = Despeckle(mask);

        return new RasterTraceImage
        {
            Mask = mask,
            WidthPixels = width,
            HeightPixels = height
        };
    }

    private static bool[,] Despeckle(bool[,] source)
    {
        var height = source.GetLength(0);
        var width = source.GetLength(1);
        var clone = (bool[,])source.Clone();

        for (var y = 1; y < height - 1; y++)
        {
            for (var x = 1; x < width - 1; x++)
            {
                if (!source[y, x])
                    continue;

                var neighbors = 0;
                for (var oy = -1; oy <= 1; oy++)
                {
                    for (var ox = -1; ox <= 1; ox++)
                    {
                        if (ox == 0 && oy == 0)
                            continue;

                        if (source[y + oy, x + ox])
                            neighbors++;
                    }
                }

                if (neighbors <= 1)
                    clone[y, x] = false;
            }
        }

        return clone;
    }
}

public sealed class OutlineVectorTraceService : IVectorTraceService
{
    public IReadOnlyList<VectorPath> Trace(RasterTraceImage image, ImageToolpathSettings settings)
    {
        var rawPaths = BuildClosedBoundaryPaths(image.Mask, image.WidthPixels, image.HeightPixels);
        var bounds = MeasureBounds(rawPaths);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return Array.Empty<VectorPath>();

        var scale = ResolveScale(settings, bounds.Width, bounds.Height);
        var tolerancePx = scale <= 0 ? 0.5 : (double)(settings.SimplifyToleranceMm / (decimal)scale);
        var output = new List<VectorPath>();

        foreach (var path in rawPaths)
        {
            var simplified = Simplify(path, tolerancePx);
            var vectorPath = new VectorPath
            {
                Closed = true
            };

            foreach (var point in simplified)
            {
                vectorPath.Points.Add(new VectorPoint
                {
                    X = (point.X - bounds.MinX) * scale,
                    Y = (point.Y - bounds.MinY) * scale
                });
            }

            BuildSegments(vectorPath);
            DetectCircle(vectorPath);
            output.Add(vectorPath);
        }

        return output;
    }

    private static (double MinX, double MinY, double Width, double Height) MeasureBounds(IReadOnlyList<List<(double X, double Y)>> paths)
    {
        if (paths.Count == 0 || paths.All(path => path.Count == 0))
            return (0d, 0d, 0d, 0d);

        var minX = paths.SelectMany(path => path).Min(point => point.X);
        var minY = paths.SelectMany(path => path).Min(point => point.Y);
        var maxX = paths.SelectMany(path => path).Max(point => point.X);
        var maxY = paths.SelectMany(path => path).Max(point => point.Y);
        return (minX, minY, maxX - minX, maxY - minY);
    }

    private static double ResolveScale(ImageToolpathSettings settings, double widthPx, double heightPx)
    {
        var targetWidth = (double)settings.TargetWidthMm;
        var targetHeight = (double)settings.TargetHeightMm;

        if (targetWidth <= 0 && targetHeight <= 0)
            return 1d;

        if (settings.PreserveAspectRatio)
        {
            if (targetWidth <= 0)
                return targetHeight / Math.Max(heightPx, 1d);
            if (targetHeight <= 0)
                return targetWidth / Math.Max(widthPx, 1d);
            return Math.Min(targetWidth / Math.Max(widthPx, 1d), targetHeight / Math.Max(heightPx, 1d));
        }

        return targetWidth > 0 ? targetWidth / Math.Max(widthPx, 1d) : targetHeight / Math.Max(heightPx, 1d);
    }

    private static List<List<(double X, double Y)>> BuildClosedBoundaryPaths(bool[,] mask, int width, int height)
    {
        var segments = new List<((double X, double Y) Start, (double X, double Y) End)>();
        bool IsFilled(int x, int y) => x >= 0 && y >= 0 && x < width && y < height && mask[y, x];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!mask[y, x])
                    continue;

                if (!IsFilled(x, y - 1))
                    segments.Add(((x, y), (x + 1d, y)));
                if (!IsFilled(x + 1, y))
                    segments.Add(((x + 1d, y), (x + 1d, y + 1d)));
                if (!IsFilled(x, y + 1))
                    segments.Add(((x + 1d, y + 1d), (x, y + 1d)));
                if (!IsFilled(x - 1, y))
                    segments.Add(((x, y + 1d), (x, y)));
            }
        }

        var paths = new List<List<(double X, double Y)>>();
        var outgoing = segments
            .GroupBy(segment => Key(segment.Start))
            .ToDictionary(group => group.Key, group => new Queue<((double X, double Y) Start, (double X, double Y) End)>(group));

        while (outgoing.Count > 0)
        {
            var first = outgoing.First();
            var segment = first.Value.Dequeue();
            if (first.Value.Count == 0)
                outgoing.Remove(first.Key);

            var path = new List<(double X, double Y)> { segment.Start, segment.End };
            var current = segment.End;

            while (true)
            {
                var key = Key(current);
                if (!outgoing.TryGetValue(key, out var nextQueue) || nextQueue.Count == 0)
                    break;

                var next = nextQueue.Dequeue();
                if (nextQueue.Count == 0)
                    outgoing.Remove(key);

                if (Distance(next.End, path[0]) < 0.0001)
                {
                    path.Add(next.End);
                    break;
                }

                path.Add(next.End);
                current = next.End;
            }

            if (path.Count >= 4)
                paths.Add(path);
        }

        return paths;
    }

    private static string Key((double X, double Y) point) => $"{point.X:0.###}|{point.Y:0.###}";

    private static List<(double X, double Y)> Simplify(List<(double X, double Y)> points, double tolerance)
    {
        if (points.Count <= 4)
            return points;

        var open = points.ToList();
        if (Distance(open[0], open[^1]) < 0.0001)
            open.RemoveAt(open.Count - 1);

        var simplified = SimplifyRdp(open, tolerance);
        if (Distance(simplified[0], simplified[^1]) >= 0.0001)
            simplified.Add(simplified[0]);

        return simplified;
    }

    private static List<(double X, double Y)> SimplifyRdp(IReadOnlyList<(double X, double Y)> points, double tolerance)
    {
        if (points.Count < 3)
            return points.ToList();

        var index = -1;
        var maxDistance = 0d;
        for (var i = 1; i < points.Count - 1; i++)
        {
            var distance = PerpendicularDistance(points[i], points[0], points[^1]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                index = i;
            }
        }

        if (maxDistance <= tolerance || index < 0)
            return new List<(double X, double Y)> { points[0], points[^1] };

        var left = SimplifyRdp(points.Take(index + 1).ToList(), tolerance);
        var right = SimplifyRdp(points.Skip(index).ToList(), tolerance);
        return left.Take(left.Count - 1).Concat(right).ToList();
    }

    private static double PerpendicularDistance((double X, double Y) point, (double X, double Y) start, (double X, double Y) end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001)
            return Distance(point, start);

        var numerator = Math.Abs((dy * point.X) - (dx * point.Y) + (end.X * start.Y) - (end.Y * start.X));
        var denominator = Math.Sqrt((dx * dx) + (dy * dy));
        return numerator / denominator;
    }

    private static double Distance((double X, double Y) a, (double X, double Y) b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static void BuildSegments(VectorPath path)
    {
        path.Segments.Clear();
        for (var i = 1; i < path.Points.Count; i++)
        {
            path.Segments.Add(new VectorSegment
            {
                Type = VectorSegmentType.Line,
                Start = new VectorPoint { X = path.Points[i - 1].X, Y = path.Points[i - 1].Y },
                End = new VectorPoint { X = path.Points[i].X, Y = path.Points[i].Y }
            });
        }
    }

    private static void DetectCircle(VectorPath path)
    {
        if (path.Points.Count < 8 || !path.Closed)
            return;

        var corePoints = path.Points.Take(path.Points.Count - 1).ToList();
        var centerX = corePoints.Average(point => point.X);
        var centerY = corePoints.Average(point => point.Y);
        var radii = corePoints.Select(point => Math.Sqrt(Math.Pow(point.X - centerX, 2) + Math.Pow(point.Y - centerY, 2))).ToList();
        var averageRadius = radii.Average();
        var deviation = radii.Max(radius => Math.Abs(radius - averageRadius));

        if (averageRadius <= 0.001 || deviation > Math.Max(averageRadius * 0.04, 0.35))
            return;

        path.IsCircleLike = true;
        path.CircleCenterX = centerX;
        path.CircleCenterY = centerY;
        path.CircleRadius = averageRadius;
    }
}

public sealed class ToolpathFromVectorService : IToolpathFromVectorService
{
    public GeneratedGcodeResult Generate(string imagePath, IReadOnlyList<VectorPath> vectorPaths, ImageToolpathSettings settings)
    {
        var result = new GeneratedGcodeResult
        {
            FileName = $"{Path.GetFileNameWithoutExtension(imagePath)}-toolpath.gcode"
        };

        if (vectorPaths.Count == 0 || vectorPaths.All(path => path.Points.Count < 2))
        {
            result.Messages.Add("[Error] No vector paths were generated from the image.");
            return result;
        }

        var minX = vectorPaths.SelectMany(path => path.Points).Min(point => point.X);
        var minY = vectorPaths.SelectMany(path => path.Points).Min(point => point.Y);
        var maxX = vectorPaths.SelectMany(path => path.Points).Max(point => point.X);
        var maxY = vectorPaths.SelectMany(path => path.Points).Max(point => point.Y);
        result.WidthMm = (decimal)(maxX - minX);
        result.HeightMm = (decimal)(maxY - minY);
        result.HasZMotion = settings.EnableZMoves && (settings.CutDepthMm != 0m || settings.SafeTravelZMm != 0m);

        var lines = new List<string>
        {
            "; Generated from image toolpath import",
            "G21",
            "G90"
        };

        if (settings.EnableZMoves)
            lines.Add($"G0 Z{Format(settings.SafeTravelZMm)}");

        var spindleOn = false;
        foreach (var path in vectorPaths.Where(path => path.Points.Count >= 2))
        {
            var start = path.Points[0];
            lines.Add($"G0 X{Format((decimal)start.X)} Y{Format((decimal)start.Y)} F{Format(settings.RapidFeedMmPerMinute)}");

            if (settings.SpindleEnabled && !spindleOn)
            {
                lines.Add("M3");
                spindleOn = true;
            }

            if (settings.EnableZMoves && settings.CutDepthMm != 0m)
                lines.Add($"G1 Z{Format(settings.CutDepthMm)} F{Format(settings.PlungeFeedMmPerMinute)}");

            if (path.IsCircleLike && path.CircleCenterX.HasValue && path.CircleCenterY.HasValue && path.CircleRadius.HasValue)
            {
                AppendCirclePath(lines, path, settings);
            }
            else
            {
                foreach (var point in path.Points.Skip(1))
                    lines.Add($"G1 X{Format((decimal)point.X)} Y{Format((decimal)point.Y)} F{Format(settings.CutFeedMmPerMinute)}");
            }

            if (settings.EnableZMoves)
                lines.Add($"G0 Z{Format(settings.SafeTravelZMm)}");
        }

        if (spindleOn)
            lines.Add("M5");

        lines.Add("M30");
        result.GcodeText = string.Join(Environment.NewLine, lines);
        result.Messages.Add($"[Info] Generated {vectorPaths.Count} traced path(s) from {Path.GetFileName(imagePath)}.");
        return result;
    }

    private static void AppendCirclePath(List<string> lines, VectorPath path, ImageToolpathSettings settings)
    {
        var points = path.Points.Take(path.Points.Count - 1).ToList();
        if (points.Count < 4 || !path.CircleCenterX.HasValue || !path.CircleCenterY.HasValue)
        {
            foreach (var point in path.Points.Skip(1))
                lines.Add($"G1 X{Format((decimal)point.X)} Y{Format((decimal)point.Y)} F{Format(settings.CutFeedMmPerMinute)}");
            return;
        }

        var start = points[0];
        var opposite = points[points.Count / 2];
        var centerX = (decimal)path.CircleCenterX.Value;
        var centerY = (decimal)path.CircleCenterY.Value;
        var iStart = centerX - (decimal)start.X;
        var jStart = centerY - (decimal)start.Y;
        var iOpposite = centerX - (decimal)opposite.X;
        var jOpposite = centerY - (decimal)opposite.Y;
        var area = SignedArea(points);
        var code = area < 0 ? "G2" : "G3";

        lines.Add($"{code} X{Format((decimal)opposite.X)} Y{Format((decimal)opposite.Y)} I{Format(iStart)} J{Format(jStart)} F{Format(settings.CutFeedMmPerMinute)}");
        lines.Add($"{code} X{Format((decimal)start.X)} Y{Format((decimal)start.Y)} I{Format(iOpposite)} J{Format(jOpposite)} F{Format(settings.CutFeedMmPerMinute)}");
    }

    private static double SignedArea(IReadOnlyList<VectorPoint> points)
    {
        var area = 0d;
        for (var i = 0; i < points.Count; i++)
        {
            var current = points[i];
            var next = points[(i + 1) % points.Count];
            area += (current.X * next.Y) - (next.X * current.Y);
        }

        return area / 2d;
    }

    private static string Format(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

public sealed class ImageToolpathService : IImageToolpathService
{
    private readonly IImagePreprocessorService _preprocessorService;
    private readonly IVectorTraceService _vectorTraceService;
    private readonly IToolpathFromVectorService _toolpathFromVectorService;

    public ImageToolpathService(
        IImagePreprocessorService preprocessorService,
        IVectorTraceService vectorTraceService,
        IToolpathFromVectorService toolpathFromVectorService)
    {
        _preprocessorService = preprocessorService;
        _vectorTraceService = vectorTraceService;
        _toolpathFromVectorService = toolpathFromVectorService;
    }

    public ImageToolpathJob CreateJob(string imagePath, ImageToolpathSettings settings)
    {
        var preprocessed = _preprocessorService.Preprocess(imagePath, settings);
        var tracedPaths = _vectorTraceService.Trace(preprocessed, settings);
        var gcode = _toolpathFromVectorService.Generate(imagePath, tracedPaths, settings);

        var job = new ImageToolpathJob
        {
            SourceImagePath = imagePath,
            Settings = settings,
            PreprocessedImage = preprocessed,
            GeneratedGcode = gcode
        };

        foreach (var path in tracedPaths)
            job.VectorPaths.Add(path);

        return job;
    }
}

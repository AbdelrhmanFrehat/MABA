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
    VectorTraceResult Trace(RasterTraceImage image, ImageToolpathSettings settings);
}

public interface IVectorImportService
{
    bool CanImport(string imagePath);
    VectorTraceResult Import(string imagePath, ImageToolpathSettings settings);
}

public interface IToolpathFromVectorService
{
    GeneratedGcodeResult Generate(string imagePath, IReadOnlyList<VectorPath> vectorPaths, ImageToolpathSettings settings);
}

public interface IImageToolpathService
{
    ImageToolpathJob CreateJob(string imagePath, ImageToolpathSettings settings);
}

public sealed class SvgVectorImportService : IVectorImportService
{
    public bool CanImport(string imagePath)
        => Path.GetExtension(imagePath).Equals(".svg", StringComparison.OrdinalIgnoreCase);

    public VectorTraceResult Import(string imagePath, ImageToolpathSettings settings)
    {
        var result = new VectorTraceResult();
        result.Warnings.Add("SVG import is prepared architecturally, but full SVG path import is not implemented yet.");
        return result;
    }
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

        return RemoveSmallComponents(clone, minimumComponentSize: 6);
    }

    private static bool[,] RemoveSmallComponents(bool[,] source, int minimumComponentSize)
    {
        var height = source.GetLength(0);
        var width = source.GetLength(1);
        var visited = new bool[height, width];
        var result = (bool[,])source.Clone();
        var offsets = new (int X, int Y)[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0),           (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (visited[y, x] || !source[y, x])
                    continue;

                var queue = new Queue<(int X, int Y)>();
                var component = new List<(int X, int Y)>();
                queue.Enqueue((x, y));
                visited[y, x] = true;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);

                    foreach (var offset in offsets)
                    {
                        var nx = current.X + offset.X;
                        var ny = current.Y + offset.Y;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        if (visited[ny, nx] || !source[ny, nx])
                            continue;

                        visited[ny, nx] = true;
                        queue.Enqueue((nx, ny));
                    }
                }

                if (component.Count >= minimumComponentSize)
                    continue;

                foreach (var point in component)
                    result[point.Y, point.X] = false;
            }
        }

        return result;
    }
}

public sealed class OutlineVectorTraceService : IVectorTraceService
{
    private readonly record struct BoundarySegment((double X, double Y) Start, (double X, double Y) End, int Direction);

    public VectorTraceResult Trace(RasterTraceImage image, ImageToolpathSettings settings)
    {
        var rawPaths = BuildClosedBoundaryPaths(image.Mask, image.WidthPixels, image.HeightPixels);
        var bounds = MeasureBounds(rawPaths);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return new VectorTraceResult();

        var scale = ResolveScale(settings, bounds.Width, bounds.Height);
        var tolerancePx = ResolvePixels(settings.SimplifyToleranceMm, scale, fallback: 0.5);
        var minSegmentPx = ResolvePixels(settings.MinimumSegmentLengthMm, scale, fallback: 0.25);
        var closeGapPx = ResolvePixels(settings.CloseGapToleranceMm, scale, fallback: 0.75);
        var minAreaPx = ResolveSquarePixels(settings.MinimumContourAreaMm2, scale, fallback: 3d);
        var output = new List<VectorPath>();
        var removedPathCount = 0;
        var segmentReduction = 0;
        var circleFitCount = 0;
        var arcFitCount = 0;
        var warnings = new List<string>();

        for (var rawIndex = 0; rawIndex < rawPaths.Count; rawIndex++)
        {
            var path = rawPaths[rawIndex];
            var cleaned = CleanupRawPath(path, closeGapPx, minSegmentPx);
            if (cleaned.Count < 3)
            {
                removedPathCount++;
                continue;
            }

            var originalCount = cleaned.Count;
            var simplified = Simplify(cleaned, tolerancePx);
            if (simplified.Count < 5)
            {
                removedPathCount++;
                continue;
            }

            segmentReduction += Math.Max(0, originalCount - simplified.Count);

            var signedArea = SignedArea(simplified);
            var absoluteArea = Math.Abs(signedArea);
            var closed = Distance(simplified[0], simplified[^1]) < closeGapPx;
            if (closed && absoluteArea < minAreaPx)
            {
                removedPathCount++;
                continue;
            }

            var vectorPath = new VectorPath
            {
                SourceIndex = rawIndex,
                Closed = closed,
                SignedArea = signedArea,
                AbsoluteArea = absoluteArea
            };

            foreach (var point in simplified)
            {
                vectorPath.Points.Add(new VectorPoint
                {
                    X = (point.X - bounds.MinX) * scale,
                    Y = (point.Y - bounds.MinY) * scale
                });
            }

            UpdateMetrics(vectorPath);
            BuildSegments(vectorPath);
            DetectCircle(vectorPath);
            DetectArc(vectorPath);
            if (vectorPath.IsCircleLike)
                circleFitCount++;
            else if (vectorPath.IsArcLike)
                arcFitCount++;
            output.Add(vectorPath);
        }

        ClassifyHierarchy(output);
        var ordered = OrderPaths(output);
        for (var index = 0; index < ordered.Count; index++)
            ordered[index].OrderIndex = index + 1;

        if (settings.ManufacturingMode is ImageManufacturingMode.ProfileInside or ImageManufacturingMode.ProfileOutside)
            warnings.Add($"{settings.ManufacturingMode} is selected, but real tool diameter compensation is not implemented yet. Paths are currently generated on-line.");

        var traceResult = new VectorTraceResult
        {
            Paths = ordered,
            RemovedPathCount = removedPathCount,
            ArcFitCount = arcFitCount,
            CircleFitCount = circleFitCount,
            SegmentReductionCount = segmentReduction
        };

        foreach (var warning in warnings)
            traceResult.Warnings.Add(warning);

        return traceResult;
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
        var segments = new List<BoundarySegment>();
        bool IsFilled(int x, int y) => x >= 0 && y >= 0 && x < width && y < height && mask[y, x];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!mask[y, x])
                    continue;

                if (!IsFilled(x, y - 1))
                    segments.Add(new BoundarySegment((x, y), (x + 1d, y), 0));
                if (!IsFilled(x + 1, y))
                    segments.Add(new BoundarySegment((x + 1d, y), (x + 1d, y + 1d), 1));
                if (!IsFilled(x, y + 1))
                    segments.Add(new BoundarySegment((x + 1d, y + 1d), (x, y + 1d), 2));
                if (!IsFilled(x - 1, y))
                    segments.Add(new BoundarySegment((x, y + 1d), (x, y), 3));
            }
        }

        var paths = new List<List<(double X, double Y)>>();
        var outgoing = segments
            .Select((segment, index) => new { segment, index })
            .GroupBy(item => Key(item.segment.Start))
            .ToDictionary(group => group.Key, group => group.Select(item => item.index).ToList());
        var used = new bool[segments.Count];

        for (var startIndex = 0; startIndex < segments.Count; startIndex++)
        {
            if (used[startIndex])
                continue;

            var segment = segments[startIndex];
            used[startIndex] = true;

            var startPoint = segment.Start;
            var path = new List<(double X, double Y)> { startPoint, segment.End };
            var current = segment.End;
            var previousDirection = segment.Direction;
            var guard = 0;

            while (guard++ < segments.Count + 4)
            {
                var key = Key(current);
                if (!outgoing.TryGetValue(key, out var candidates))
                    break;

                var nextIndex = SelectNextSegment(candidates, segments, used, previousDirection, startPoint);
                if (nextIndex < 0)
                    break;

                var next = segments[nextIndex];
                used[nextIndex] = true;
                path.Add(next.End);
                current = next.End;
                previousDirection = next.Direction;

                if (Distance(current, startPoint) < 0.0001)
                    break;
            }

            if (path.Count >= 4 && Distance(path[0], path[^1]) < 0.0001)
                paths.Add(path);
        }

        return paths
            .OrderByDescending(path => Math.Abs(SignedArea(path)))
            .ToList();
    }

    private static List<(double X, double Y)> CleanupRawPath(
        List<(double X, double Y)> points,
        double closeGapTolerance,
        double minimumSegmentLength)
    {
        var cleaned = new List<(double X, double Y)>();
        foreach (var point in points)
        {
            if (cleaned.Count == 0 || Distance(cleaned[^1], point) >= minimumSegmentLength)
                cleaned.Add(point);
        }

        if (cleaned.Count >= 2 && Distance(cleaned[0], cleaned[^1]) <= closeGapTolerance)
            cleaned[^1] = cleaned[0];

        return cleaned;
    }

    private static string Key((double X, double Y) point) => $"{point.X:0.###}|{point.Y:0.###}";

    private static int SelectNextSegment(
        IReadOnlyList<int> candidateIndexes,
        IReadOnlyList<BoundarySegment> segments,
        IReadOnlyList<bool> used,
        int previousDirection,
        (double X, double Y) startPoint)
    {
        var selected = -1;
        var bestPriority = int.MaxValue;
        var bestClosure = -1;

        foreach (var candidateIndex in candidateIndexes)
        {
            if (used[candidateIndex])
                continue;

            var candidate = segments[candidateIndex];
            var turn = (candidate.Direction - previousDirection + 4) % 4;
            var priority = turn switch
            {
                1 => 0, // keep the contour hugging the shape
                0 => 1,
                3 => 2,
                _ => 3
            };
            var closesLoop = Distance(candidate.End, startPoint) < 0.0001 ? 1 : 0;

            if (priority < bestPriority || (priority == bestPriority && closesLoop > bestClosure))
            {
                bestPriority = priority;
                bestClosure = closesLoop;
                selected = candidateIndex;
            }
        }

        return selected;
    }

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

    private static double SignedArea(IReadOnlyList<(double X, double Y)> points)
    {
        var area = 0d;
        for (var i = 0; i < points.Count - 1; i++)
            area += (points[i].X * points[i + 1].Y) - (points[i + 1].X * points[i].Y);

        return area / 2d;
    }

    private static double ResolvePixels(decimal millimeters, double scale, double fallback)
    {
        if (scale <= 0 || millimeters <= 0m)
            return fallback;

        return (double)(millimeters / (decimal)scale);
    }

    private static double ResolveSquarePixels(decimal squareMillimeters, double scale, double fallback)
    {
        if (scale <= 0 || squareMillimeters <= 0m)
            return fallback;

        var pixels = (double)(squareMillimeters / (decimal)(scale * scale));
        return pixels > 0d ? pixels : fallback;
    }

    private static void UpdateMetrics(VectorPath path)
    {
        if (path.Points.Count == 0)
            return;

        path.MinX = path.Points.Min(point => point.X);
        path.MinY = path.Points.Min(point => point.Y);
        path.MaxX = path.Points.Max(point => point.X);
        path.MaxY = path.Points.Max(point => point.Y);
        path.CentroidX = (path.MinX + path.MaxX) / 2d;
        path.CentroidY = (path.MinY + path.MaxY) / 2d;
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

    private static void DetectArc(VectorPath path)
    {
        if (path.Closed || path.Points.Count < 4 || path.IsCircleLike)
            return;

        var start = path.Points.First();
        var middle = path.Points[path.Points.Count / 2];
        var end = path.Points.Last();
        if (!TryFitCircle(start, middle, end, out var centerX, out var centerY, out var radius))
            return;

        var maxDeviation = path.Points.Max(point => Math.Abs(Math.Sqrt(Math.Pow(point.X - centerX, 2) + Math.Pow(point.Y - centerY, 2)) - radius));
        if (maxDeviation > Math.Max(radius * 0.02, 0.2))
            return;

        path.IsArcLike = true;
        path.ArcCenterX = centerX;
        path.ArcCenterY = centerY;
        path.ArcRadius = radius;
        path.ArcClockwise = Cross(start, middle, end) < 0d;
    }

    private static bool TryFitCircle(VectorPoint start, VectorPoint middle, VectorPoint end, out double centerX, out double centerY, out double radius)
    {
        var ax = start.X;
        var ay = start.Y;
        var bx = middle.X;
        var by = middle.Y;
        var cx = end.X;
        var cy = end.Y;

        var d = 2d * ((ax * (by - cy)) + (bx * (cy - ay)) + (cx * (ay - by)));
        if (Math.Abs(d) < 0.0001)
        {
            centerX = centerY = radius = 0d;
            return false;
        }

        centerX = (((ax * ax + ay * ay) * (by - cy)) + ((bx * bx + by * by) * (cy - ay)) + ((cx * cx + cy * cy) * (ay - by))) / d;
        centerY = (((ax * ax + ay * ay) * (cx - bx)) + ((bx * bx + by * by) * (ax - cx)) + ((cx * cx + cy * cy) * (bx - ax))) / d;
        radius = Math.Sqrt(Math.Pow(ax - centerX, 2) + Math.Pow(ay - centerY, 2));
        return radius > 0.0001;
    }

    private static double Cross(VectorPoint a, VectorPoint b, VectorPoint c)
        => ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));

    private static void ClassifyHierarchy(List<VectorPath> paths)
    {
        foreach (var path in paths)
        {
            path.ParentPathIndex = null;
            path.NestingDepth = 0;
            path.Kind = path.Closed ? VectorPathKind.OuterContour : VectorPathKind.OpenStroke;
        }

        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            if (!path.Closed)
                continue;

            int? parentIndex = null;
            double? parentArea = null;
            for (var candidateIndex = 0; candidateIndex < paths.Count; candidateIndex++)
            {
                if (candidateIndex == i)
                    continue;

                var candidate = paths[candidateIndex];
                if (!candidate.Closed || candidate.AbsoluteArea <= path.AbsoluteArea)
                    continue;

                if (!ContainsPoint(candidate, path.CentroidX, path.CentroidY))
                    continue;

                if (parentArea == null || candidate.AbsoluteArea < parentArea)
                {
                    parentIndex = candidateIndex;
                    parentArea = candidate.AbsoluteArea;
                }
            }

            paths[i].ParentPathIndex = parentIndex;
        }

        foreach (var path in paths.Where(path => path.Closed))
        {
            var depth = 0;
            var parent = path.ParentPathIndex;
            while (parent.HasValue)
            {
                depth++;
                parent = paths[parent.Value].ParentPathIndex;
            }

            path.NestingDepth = depth;
            path.Kind = depth switch
            {
                0 => VectorPathKind.OuterContour,
                _ when depth % 2 == 1 => VectorPathKind.InnerHole,
                _ => VectorPathKind.Island
            };
        }
    }

    private static List<VectorPath> OrderPaths(List<VectorPath> paths)
    {
        var ordered = new List<VectorPath>();
        var roots = paths.Where(path => path.ParentPathIndex == null && path.Kind != VectorPathKind.OpenStroke).ToList();
        var currentX = 0d;
        var currentY = 0d;

        OrderSiblings(roots, ref currentX, ref currentY, paths, ordered);

        var openStrokes = paths.Where(path => path.Kind == VectorPathKind.OpenStroke && !ordered.Contains(path)).ToList();
        if (openStrokes.Count > 0)
            OrderSiblings(openStrokes, ref currentX, ref currentY, paths, ordered);

        return ordered;
    }

    private static void OrderSiblings(
        List<VectorPath> siblings,
        ref double currentX,
        ref double currentY,
        IReadOnlyList<VectorPath> allPaths,
        List<VectorPath> ordered)
    {
        var remaining = siblings.ToList();
        while (remaining.Count > 0)
        {
            var anchorX = currentX;
            var anchorY = currentY;
            var next = remaining
                .OrderBy(path => Distance((path.Points[0].X, path.Points[0].Y), (anchorX, anchorY)))
                .First();
            remaining.Remove(next);

            var children = allPaths.Where(path => path.ParentPathIndex == next.SourceIndex && !ordered.Contains(path)).ToList();
            if (children.Count > 0)
                OrderSiblings(children, ref currentX, ref currentY, allPaths, ordered);

            if (!ordered.Contains(next))
                ordered.Add(next);

            currentX = next.Points.Last().X;
            currentY = next.Points.Last().Y;
        }

    }

    private static bool ContainsPoint(VectorPath polygon, double x, double y)
    {
        var inside = false;
        var points = polygon.Points.ToList();
        if (points.Count < 3)
            return false;

        for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
        {
            var pi = points[i];
            var pj = points[j];
            var intersects = ((pi.Y > y) != (pj.Y > y))
                             && (x < ((pj.X - pi.X) * (y - pi.Y) / Math.Max(pj.Y - pi.Y, 0.000001)) + pi.X);
            if (intersects)
                inside = !inside;
        }

        return inside;
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
        var rapidDistance = 0m;
        var cutDistance = 0m;

        var lines = new List<string>
        {
            "; Generated from image toolpath import",
            "G21",
            "G90"
        };

        if (settings.EnableZMoves)
            lines.Add($"G0 Z{Format(settings.SafeTravelZMm)}");

        var spindleOn = false;
        VectorPoint? previousEnd = null;
        foreach (var path in vectorPaths.Where(path => path.Points.Count >= 2))
        {
            var start = path.Points[0];
            if (previousEnd != null)
                rapidDistance += DistanceMm(previousEnd, start);

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
                cutDistance += AppendCirclePath(lines, path, settings);
            }
            else if (path.IsArcLike && path.ArcCenterX.HasValue && path.ArcCenterY.HasValue && path.ArcRadius.HasValue)
            {
                cutDistance += AppendArcPath(lines, path, settings);
            }
            else
            {
                foreach (var point in path.Points.Skip(1))
                {
                    lines.Add($"G1 X{Format((decimal)point.X)} Y{Format((decimal)point.Y)} F{Format(settings.CutFeedMmPerMinute)}");
                    cutDistance += DistanceMm(path.Points[path.Points.IndexOf(point) - 1], point);
                }
            }

            if (settings.EnableZMoves)
                lines.Add($"G0 Z{Format(settings.SafeTravelZMm)}");

            previousEnd = path.Points.Last();
        }

        if (spindleOn)
            lines.Add("M5");

        lines.Add("M30");
        result.GcodeText = string.Join(Environment.NewLine, lines);
        result.TotalCutDistanceMm = cutDistance;
        result.TotalRapidDistanceMm = rapidDistance;
        result.EstimatedJobTime = EstimateTime(cutDistance, rapidDistance, settings);
        result.GcodeLineCount = lines.Count;
        result.Messages.Add($"[Info] Generated {vectorPaths.Count} traced path(s) from {Path.GetFileName(imagePath)}.");
        if (settings.ManufacturingMode is ImageManufacturingMode.ProfileInside or ImageManufacturingMode.ProfileOutside)
            result.Messages.Add("[Warning] Profile mode selected, but real tool diameter compensation is not implemented yet. Toolpath remains on-line.");
        return result;
    }

    private static decimal AppendCirclePath(List<string> lines, VectorPath path, ImageToolpathSettings settings)
    {
        var points = path.Points.Take(path.Points.Count - 1).ToList();
        if (points.Count < 4 || !path.CircleCenterX.HasValue || !path.CircleCenterY.HasValue)
        {
            var distance = 0m;
            foreach (var point in path.Points.Skip(1))
            {
                lines.Add($"G1 X{Format((decimal)point.X)} Y{Format((decimal)point.Y)} F{Format(settings.CutFeedMmPerMinute)}");
                distance += DistanceMm(path.Points[path.Points.IndexOf(point) - 1], point);
            }
            return distance;
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
        return (decimal)(2d * Math.PI * (path.CircleRadius ?? 0d));
    }

    private static decimal AppendArcPath(List<string> lines, VectorPath path, ImageToolpathSettings settings)
    {
        var start = path.Points.First();
        var end = path.Points.Last();
        var centerX = (decimal)(path.ArcCenterX ?? 0d);
        var centerY = (decimal)(path.ArcCenterY ?? 0d);
        var i = centerX - (decimal)start.X;
        var j = centerY - (decimal)start.Y;
        var code = path.ArcClockwise ? "G2" : "G3";
        lines.Add($"{code} X{Format((decimal)end.X)} Y{Format((decimal)end.Y)} I{Format(i)} J{Format(j)} F{Format(settings.CutFeedMmPerMinute)}");

        var startAngle = Math.Atan2(start.Y - (path.ArcCenterY ?? 0d), start.X - (path.ArcCenterX ?? 0d));
        var endAngle = Math.Atan2(end.Y - (path.ArcCenterY ?? 0d), end.X - (path.ArcCenterX ?? 0d));
        var sweep = NormalizeSweep(startAngle, endAngle, path.ArcClockwise);
        return (decimal)Math.Abs(sweep) * (decimal)(path.ArcRadius ?? 0d);
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

    private static decimal DistanceMm(VectorPoint a, VectorPoint b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (decimal)Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static TimeSpan EstimateTime(decimal cutDistance, decimal rapidDistance, ImageToolpathSettings settings)
    {
        var cutMinutes = settings.CutFeedMmPerMinute > 0m ? cutDistance / settings.CutFeedMmPerMinute : 0m;
        var rapidMinutes = settings.RapidFeedMmPerMinute > 0m ? rapidDistance / settings.RapidFeedMmPerMinute : 0m;
        var plungeMinutes = settings.EnableZMoves && settings.PlungeFeedMmPerMinute > 0m
            ? Math.Abs(settings.CutDepthMm) * 1.2m / settings.PlungeFeedMmPerMinute
            : 0m;
        return TimeSpan.FromMinutes((double)(cutMinutes + rapidMinutes + plungeMinutes));
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

    private static string Format(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

public sealed class ImageToolpathService : IImageToolpathService
{
    private readonly IImagePreprocessorService _preprocessorService;
    private readonly IVectorTraceService _vectorTraceService;
    private readonly IToolpathFromVectorService _toolpathFromVectorService;
    private readonly IVectorImportService _vectorImportService;

    public ImageToolpathService(
        IImagePreprocessorService preprocessorService,
        IVectorTraceService vectorTraceService,
        IToolpathFromVectorService toolpathFromVectorService,
        IVectorImportService? vectorImportService = null)
    {
        _preprocessorService = preprocessorService;
        _vectorTraceService = vectorTraceService;
        _toolpathFromVectorService = toolpathFromVectorService;
        _vectorImportService = vectorImportService ?? new SvgVectorImportService();
    }

    public ImageToolpathJob CreateJob(string imagePath, ImageToolpathSettings settings)
    {
        RasterTraceImage? preprocessed = null;
        VectorTraceResult traceResult;
        if (_vectorImportService.CanImport(imagePath))
        {
            traceResult = _vectorImportService.Import(imagePath, settings);
        }
        else
        {
            preprocessed = _preprocessorService.Preprocess(imagePath, settings);
            traceResult = _vectorTraceService.Trace(preprocessed, settings);
        }

        var gcode = _toolpathFromVectorService.Generate(imagePath, traceResult.Paths, settings);

        var job = new ImageToolpathJob
        {
            SourceImagePath = imagePath,
            Settings = settings,
            PreprocessedImage = preprocessed,
            GeneratedGcode = gcode,
            Diagnostics = BuildDiagnostics(traceResult, gcode),
            Preview = BuildPreview(traceResult.Paths)
        };

        foreach (var path in traceResult.Paths)
            job.VectorPaths.Add(path);

        foreach (var warning in traceResult.Warnings)
            job.GeneratedGcode.Messages.Add($"[Warning] {warning}");

        return job;
    }

    private static ImageToolpathDiagnostics BuildDiagnostics(VectorTraceResult traceResult, GeneratedGcodeResult gcode)
    {
        var diagnostics = new ImageToolpathDiagnostics
        {
            TracedPathCount = traceResult.Paths.Count,
            RemovedPathCount = traceResult.RemovedPathCount,
            ClosedContourCount = traceResult.Paths.Count(path => path.Closed),
            OpenStrokeCount = traceResult.Paths.Count(path => path.Kind == VectorPathKind.OpenStroke),
            HoleCount = traceResult.Paths.Count(path => path.Kind == VectorPathKind.InnerHole),
            ArcFitCount = traceResult.ArcFitCount,
            CircleFitCount = traceResult.CircleFitCount,
            SegmentReductionCount = traceResult.SegmentReductionCount,
            TotalCutDistanceMm = gcode.TotalCutDistanceMm,
            TotalRapidDistanceMm = gcode.TotalRapidDistanceMm,
            EstimatedJobTime = gcode.EstimatedJobTime,
            GcodeLineCount = gcode.GcodeLineCount
        };

        foreach (var warning in traceResult.Warnings)
            diagnostics.Warnings.Add(warning);

        foreach (var message in gcode.Messages.Where(message => message.Contains("[Warning]", StringComparison.OrdinalIgnoreCase)))
            diagnostics.Warnings.Add(message);

        return diagnostics;
    }

    private static ImageToolpathPreview BuildPreview(IReadOnlyList<VectorPath> paths)
    {
        var preview = new ImageToolpathPreview();
        if (paths.Count == 0)
            return preview;

        var cutBuilder = new StringBuilder();
        var rapidBuilder = new StringBuilder();
        var minX = paths.Min(path => path.MinX);
        var minY = paths.Min(path => path.MinY);
        var maxX = paths.Max(path => path.MaxX);
        var maxY = paths.Max(path => path.MaxY);
        preview.BoundingBoxGeometryData = $"M {minX:0.###},{minY:0.###} L {maxX:0.###},{minY:0.###} L {maxX:0.###},{maxY:0.###} L {minX:0.###},{maxY:0.###} Z";

        VectorPoint? previousEnd = null;
        foreach (var path in paths.Where(path => path.Points.Count > 1))
        {
            var first = path.Points[0];
            if (previousEnd != null)
                rapidBuilder.Append($"M {previousEnd.X:0.###},{previousEnd.Y:0.###} L {first.X:0.###},{first.Y:0.###} ");

            cutBuilder.Append($"M {first.X:0.###},{first.Y:0.###} ");
            foreach (var point in path.Points.Skip(1))
                cutBuilder.Append($"L {point.X:0.###},{point.Y:0.###} ");
            if (path.Closed)
                cutBuilder.Append("Z ");

            preview.Markers.Add(new ImagePreviewMarker
            {
                Label = path.OrderIndex.ToString(CultureInfo.InvariantCulture),
                X = first.X,
                Y = first.Y,
                Kind = ImagePreviewMarkerKind.Order
            });
            preview.Markers.Add(new ImagePreviewMarker
            {
                Label = "S",
                X = first.X,
                Y = first.Y,
                Kind = ImagePreviewMarkerKind.Start
            });
            preview.Markers.Add(new ImagePreviewMarker
            {
                Label = "E",
                X = path.Points.Last().X,
                Y = path.Points.Last().Y,
                Kind = ImagePreviewMarkerKind.End
            });
            preview.Markers.Add(new ImagePreviewMarker
            {
                Label = "P",
                X = first.X,
                Y = first.Y,
                Kind = ImagePreviewMarkerKind.Plunge
            });
            preview.Markers.Add(new ImagePreviewMarker
            {
                Label = "R",
                X = path.Points.Last().X,
                Y = path.Points.Last().Y,
                Kind = ImagePreviewMarkerKind.Retract
            });

            previousEnd = path.Points.Last();
        }

        preview.CutGeometryData = cutBuilder.ToString().Trim();
        preview.RapidGeometryData = rapidBuilder.ToString().Trim();
        return preview;
    }
}

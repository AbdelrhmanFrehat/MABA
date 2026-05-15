using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
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
        var document = XDocument.Load(imagePath);
        var root = document.Root;
        if (root == null)
        {
            var empty = new VectorTraceResult();
            empty.Warnings.Add("SVG file is empty.");
            return empty;
        }

        var rootTransform = ParseSvgTransform(root.Attribute("transform")?.Value, out var rootTransformWarnings);
        var sourcePaths = new List<List<(double X, double Y)>>();
        var warnings = new List<string>();
        warnings.AddRange(rootTransformWarnings);
        var sourceIndex = 0;

        foreach (var element in root.Descendants())
        {
            var localName = element.Name.LocalName;
            Geometry? geometry = localName switch
            {
                "path" => CreatePathGeometry(element),
                "rect" => CreateRectGeometry(element),
                "circle" => CreateCircleGeometry(element),
                "ellipse" => CreateEllipseGeometry(element),
                "line" => CreateLineGeometry(element),
                "polyline" => CreatePolylineGeometry(element, closed: false),
                "polygon" => CreatePolylineGeometry(element, closed: true),
                _ => null
            };

            if (geometry == null)
                continue;

            var combinedTransform = rootTransform;
            foreach (var ancestor in element.AncestorsAndSelf().Reverse())
            {
                if (ancestor == root)
                    continue;

                var transform = ParseSvgTransform(ancestor.Attribute("transform")?.Value, out var transformWarnings);
                warnings.AddRange(transformWarnings.Select(warning => $"{localName}: {warning}"));
                combinedTransform = Matrix.Multiply(combinedTransform, transform);
            }

            if (!combinedTransform.IsIdentity)
            {
                geometry = geometry.Clone();
                geometry.Transform = new MatrixTransform(combinedTransform);
            }

            var flattened = geometry.GetFlattenedPathGeometry(0.1, ToleranceType.Absolute);
            foreach (var figure in flattened.Figures)
            {
                var figurePoints = new List<(double X, double Y)>
                {
                    (figure.StartPoint.X, figure.StartPoint.Y)
                };

                foreach (var segment in figure.Segments)
                {
                    if (segment is PolyLineSegment poly)
                    {
                        foreach (var point in poly.Points)
                        {
                            figurePoints.Add((point.X, point.Y));
                        }
                    }
                    else if (segment is LineSegment line)
                    {
                        figurePoints.Add((line.Point.X, line.Point.Y));
                    }
                }

                if (figure.IsClosed && figurePoints.Count > 2)
                    figurePoints.Add((figurePoints[0].X, figurePoints[0].Y));

                if (figurePoints.Count < 2)
                    continue;

                var numericPoints = figurePoints.ToList();
                numericPoints = OutlineVectorTraceService.CleanupRawPath(numericPoints, (double)settings.CloseGapToleranceMm, (double)settings.MinimumSegmentLengthMm);
                if (numericPoints.Count < 2)
                    continue;

                sourcePaths.Add(numericPoints);
                sourceIndex++;
            }
        }

        if (sourcePaths.Count == 0)
        {
            var empty = new VectorTraceResult { TraceEngine = "SVG import" };
            foreach (var warning in warnings)
                empty.Warnings.Add(warning);
            return empty;
        }

        var bounds = MeasureImportedBounds(sourcePaths);
        var sourceWidth = Math.Max(bounds.Width, 1d);
        var sourceHeight = Math.Max(bounds.Height, 1d);
        var scaleX = settings.TargetWidthMm > 0m ? (double)settings.TargetWidthMm / sourceWidth : 1d;
        var scaleY = settings.TargetHeightMm > 0m ? (double)settings.TargetHeightMm / sourceHeight : 1d;
        if (settings.PreserveAspectRatio)
        {
            var uniform = Math.Min(scaleX > 0 ? scaleX : scaleY, scaleY > 0 ? scaleY : scaleX);
            scaleX = uniform > 0 ? uniform : 1d;
            scaleY = scaleX;
        }

        var paths = new List<VectorPath>();
        for (var index = 0; index < sourcePaths.Count; index++)
        {
            var numericPoints = sourcePaths[index]
                .Select(point => (X: (point.X - bounds.MinX) * scaleX, Y: (point.Y - bounds.MinY) * scaleY))
                .ToList();
            numericPoints = OutlineVectorTraceService.SmoothPath(numericPoints, settings.SmoothingAmount);
            numericPoints = OutlineVectorTraceService.MergeCollinear(numericPoints, (double)settings.MinimumSegmentLengthMm);
            numericPoints = OutlineVectorTraceService.Simplify(numericPoints, Math.Max((double)settings.SimplifyToleranceMm, 0.05d));
            numericPoints = OutlineVectorTraceService.CleanupRawPath(numericPoints, (double)settings.CloseGapToleranceMm, (double)settings.MinimumSegmentLengthMm);
            if (numericPoints.Count < 2)
                continue;

            var path = new VectorPath
            {
                SourceIndex = index,
                Closed = OutlineVectorTraceService.Distance((numericPoints[0].X, numericPoints[0].Y), (numericPoints[^1].X, numericPoints[^1].Y)) <= (double)settings.CloseGapToleranceMm
            };

            foreach (var point in numericPoints)
                path.Points.Add(new VectorPoint { X = point.X, Y = point.Y });

            path.SignedArea = OutlineVectorTraceService.SignedArea(path.Points.Select(point => (point.X, point.Y)).ToList());
            path.AbsoluteArea = Math.Abs(path.SignedArea);
            OutlineVectorTraceService.UpdateMetrics(path);
            OutlineVectorTraceService.BuildSegments(path);
            paths.Add(path);
        }

        var joinedPathCount = 0;
        var lineFitCount = 0;
        var arcFitCount = 0;
        var circleFitCount = 0;
        paths = OutlineVectorTraceService.JoinOpenStrokes(paths, (double)settings.CloseGapToleranceMm, ref joinedPathCount);
        OutlineVectorTraceService.ClassifyHierarchy(paths);
        foreach (var path in paths)
        {
            if (OutlineVectorTraceService.TryCollapseLine(path))
                lineFitCount++;

            OutlineVectorTraceService.DetectCircle(path);
            if (path.IsCircleLike)
            {
                circleFitCount++;
                continue;
            }

            OutlineVectorTraceService.DetectArc(path);
            if (path.IsArcLike)
                arcFitCount++;
        }

        paths = OutlineVectorTraceService.OrderPaths(paths);
        for (var index = 0; index < paths.Count; index++)
            paths[index].OrderIndex = index + 1;

        var result = new VectorTraceResult
        {
            TraceEngine = "SVG import",
            Paths = paths,
            JoinedPathCount = joinedPathCount,
            LineFitCount = lineFitCount,
            ArcFitCount = arcFitCount,
            CircleFitCount = circleFitCount
        };

        foreach (var warning in warnings)
            result.Warnings.Add(warning);
        if (settings.ManufacturingMode is ImageManufacturingMode.ProfileInside or ImageManufacturingMode.ProfileOutside)
            result.Warnings.Add($"{settings.ManufacturingMode} is selected, but real tool diameter compensation is not implemented yet. Paths are currently generated on-line.");

        return result;
    }

    private static Geometry? CreatePathGeometry(XElement element)
    {
        var data = element.Attribute("d")?.Value;
        if (string.IsNullOrWhiteSpace(data))
            return null;

        try
        {
            return Geometry.Parse(data);
        }
        catch
        {
            return null;
        }
    }

    private static Geometry? CreateRectGeometry(XElement element)
    {
        var x = ParseSvgLength(element.Attribute("x")?.Value) ?? 0d;
        var y = ParseSvgLength(element.Attribute("y")?.Value) ?? 0d;
        var width = ParseSvgLength(element.Attribute("width")?.Value) ?? 0d;
        var height = ParseSvgLength(element.Attribute("height")?.Value) ?? 0d;
        if (width <= 0d || height <= 0d)
            return null;

        return new RectangleGeometry(new Rect(x, y, width, height));
    }

    private static Geometry? CreateCircleGeometry(XElement element)
    {
        var cx = ParseSvgLength(element.Attribute("cx")?.Value) ?? 0d;
        var cy = ParseSvgLength(element.Attribute("cy")?.Value) ?? 0d;
        var r = ParseSvgLength(element.Attribute("r")?.Value) ?? 0d;
        return r > 0d ? new EllipseGeometry(new Point(cx, cy), r, r) : null;
    }

    private static Geometry? CreateEllipseGeometry(XElement element)
    {
        var cx = ParseSvgLength(element.Attribute("cx")?.Value) ?? 0d;
        var cy = ParseSvgLength(element.Attribute("cy")?.Value) ?? 0d;
        var rx = ParseSvgLength(element.Attribute("rx")?.Value) ?? 0d;
        var ry = ParseSvgLength(element.Attribute("ry")?.Value) ?? 0d;
        return rx > 0d && ry > 0d ? new EllipseGeometry(new Point(cx, cy), rx, ry) : null;
    }

    private static Geometry? CreateLineGeometry(XElement element)
    {
        var x1 = ParseSvgLength(element.Attribute("x1")?.Value) ?? 0d;
        var y1 = ParseSvgLength(element.Attribute("y1")?.Value) ?? 0d;
        var x2 = ParseSvgLength(element.Attribute("x2")?.Value) ?? 0d;
        var y2 = ParseSvgLength(element.Attribute("y2")?.Value) ?? 0d;
        return new LineGeometry(new Point(x1, y1), new Point(x2, y2));
    }

    private static Geometry? CreatePolylineGeometry(XElement element, bool closed)
    {
        var pointsValue = element.Attribute("points")?.Value;
        if (string.IsNullOrWhiteSpace(pointsValue))
            return null;

        var points = ParsePointList(pointsValue);
        if (points.Count < 2)
            return null;

        var figure = new PathFigure { StartPoint = points[0], IsClosed = closed, IsFilled = closed };
        var polyLine = new PolyLineSegment();
        foreach (var point in points.Skip(1))
            polyLine.Points.Add(point);
        figure.Segments.Add(polyLine);
        return new PathGeometry(new[] { figure });
    }

    private static List<Point> ParsePointList(string pointsValue)
    {
        var tokens = pointsValue
            .Replace(",", " ")
            .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var points = new List<Point>();
        for (var i = 0; i + 1 < tokens.Length; i += 2)
        {
            if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(tokens[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                points.Add(new Point(x, y));
            }
        }

        return points;
    }

    private static (double MinX, double MinY, double Width, double Height) ParseViewBox(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (0d, 0d, 100d, 100d);

        var tokens = value
            .Replace(",", " ")
            .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 4)
            return (0d, 0d, 100d, 100d);

        return (
            ParseSvgLength(tokens[0]) ?? 0d,
            ParseSvgLength(tokens[1]) ?? 0d,
            ParseSvgLength(tokens[2]) ?? 100d,
            ParseSvgLength(tokens[3]) ?? 100d);
    }

    private static double? ParseSvgLength(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = new string(value.Trim().TakeWhile(ch => char.IsDigit(ch) || ch is '.' or '-' or '+' or 'e' or 'E').ToArray());
        return double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static Matrix ParseSvgTransform(string? value, out List<string> warnings)
    {
        warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(value))
            return Matrix.Identity;

        var matrix = Matrix.Identity;
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(value, @"([a-zA-Z]+)\s*\(([^)]*)\)"))
        {
            var name = match.Groups[1].Value.Trim();
            var args = match.Groups[2].Value
                .Replace(",", " ")
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : (double?)null)
                .ToList();

            if (args.Any(argument => argument == null))
            {
                warnings.Add($"SVG transform '{name}' could not be parsed and was skipped.");
                continue;
            }

            var next = CreateSvgTransformMatrix(name, args);

            if (next.IsIdentity && name is not ("translate" or "scale" or "rotate" or "matrix"))
                warnings.Add($"SVG transform '{name}' is not supported yet and was skipped.");

            matrix = Matrix.Multiply(matrix, next);
        }

        return matrix;
    }

    private static (double MinX, double MinY, double Width, double Height) MeasureImportedBounds(IReadOnlyList<List<(double X, double Y)>> paths)
    {
        if (paths.Count == 0 || paths.All(path => path.Count == 0))
            return (0d, 0d, 0d, 0d);

        var minX = paths.SelectMany(path => path).Min(point => point.X);
        var minY = paths.SelectMany(path => path).Min(point => point.Y);
        var maxX = paths.SelectMany(path => path).Max(point => point.X);
        var maxY = paths.SelectMany(path => path).Max(point => point.Y);
        return (minX, minY, maxX - minX, maxY - minY);
    }

    private static Matrix CreateSvgTransformMatrix(string name, List<double?> args)
    {
        var matrix = Matrix.Identity;
        switch (name)
        {
            case "translate" when args.Count >= 1:
                matrix.Translate(args[0]!.Value, args.Count >= 2 ? args[1]!.Value : 0d);
                return matrix;
            case "scale" when args.Count >= 1:
                matrix.Scale(args[0]!.Value, args.Count >= 2 ? args[1]!.Value : args[0]!.Value);
                return matrix;
            case "rotate" when args.Count == 1:
                matrix.Rotate(args[0]!.Value);
                return matrix;
            case "rotate" when args.Count >= 3:
                matrix.RotateAt(args[0]!.Value, args[1]!.Value, args[2]!.Value);
                return matrix;
            case "matrix" when args.Count >= 6:
                return new Matrix(args[0]!.Value, args[1]!.Value, args[2]!.Value, args[3]!.Value, args[4]!.Value, args[5]!.Value);
            default:
                return Matrix.Identity;
        }
    }
}

public sealed class PotraceVectorTraceService : IVectorTraceService
{
    private readonly IVectorTraceService _fallbackTraceService;
    private readonly IVectorImportService _svgImportService;
    private readonly string? _potracePath;

    public PotraceVectorTraceService(IVectorTraceService fallbackTraceService, IVectorImportService svgImportService, string? potracePath = null)
    {
        _fallbackTraceService = fallbackTraceService;
        _svgImportService = svgImportService;
        _potracePath = potracePath;
    }

    public VectorTraceResult Trace(RasterTraceImage image, ImageToolpathSettings settings)
    {
        if (settings.TraceMode == ImageTraceMode.Centerline)
        {
            var fallback = _fallbackTraceService.Trace(image, settings);
            fallback.TraceEngine = "Legacy contour tracer";
            fallback.Warnings.Add("Centerline tracing still uses the legacy contour tracer.");
            return fallback;
        }

        var potracePath = ResolvePotracePath();
        if (string.IsNullOrWhiteSpace(potracePath) || !File.Exists(potracePath))
        {
            var fallback = _fallbackTraceService.Trace(image, settings);
            fallback.TraceEngine = "Legacy contour tracer";
            fallback.Warnings.Add("Potrace is unavailable, so bitmap tracing fell back to the legacy contour tracer.");
            return fallback;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "MabaControlCenter", "potrace", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var inputPath = Path.Combine(tempDirectory, "input.pbm");
            var outputPath = Path.Combine(tempDirectory, "output.svg");
            WritePortableBitmap(image.Mask, image.WidthPixels, image.HeightPixels, inputPath);

            var turdSize = Math.Max(2, (int)Math.Round(Math.Max((double)settings.MinimumFeatureSizeMm * 2.5d, 2d)));
            var alphaMax = Math.Clamp(1d + ((double)settings.SmoothingAmount * 1.5d), 0d, 1.333d);
            var optTolerance = Math.Clamp(Math.Max((double)settings.SimplifyToleranceMm, 0.15d), 0.1d, 2d);
            var arguments = $"-s --tight --group --turdsize {turdSize} --alphamax {alphaMax.ToString("0.###", CultureInfo.InvariantCulture)} --opttolerance {optTolerance.ToString("0.###", CultureInfo.InvariantCulture)} -o \"{outputPath}\" \"{inputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = potracePath,
                Arguments = arguments,
                WorkingDirectory = tempDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException("Potrace could not be started.");

            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(120000))
            {
                try { process.Kill(true); } catch { }
                throw new TimeoutException("Potrace timed out while tracing the image.");
            }

            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                var details = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
                throw new InvalidOperationException($"Potrace failed with exit code {process.ExitCode}. {details}".Trim());
            }

            var result = _svgImportService.Import(outputPath, settings);
            result.TraceEngine = "Potrace 1.16";

            if (settings.TraceMode == ImageTraceMode.TechnicalDrawing)
            {
                var warnings = new List<string>(result.Warnings);
                var filtered = OutlineVectorTraceService.FilterTechnicalDrawingContours(result.Paths.ToList(), warnings);
                var ordered = OutlineVectorTraceService.OrderPaths(filtered);
                for (var index = 0; index < ordered.Count; index++)
                    ordered[index].OrderIndex = index + 1;

                result = new VectorTraceResult
                {
                    TraceEngine = "Potrace 1.16",
                    Paths = ordered,
                    RemovedPathCount = result.RemovedPathCount,
                    JoinedPathCount = result.JoinedPathCount,
                    LineFitCount = result.LineFitCount,
                    ArcFitCount = result.ArcFitCount,
                    CircleFitCount = result.CircleFitCount,
                    SegmentReductionCount = result.SegmentReductionCount,
                    RemovedArtifactCount = result.RemovedArtifactCount,
                    MergedSegmentCount = result.MergedSegmentCount,
                    SimplifiedPointCount = result.SimplifiedPointCount,
                    RejectedTinyContourCount = result.RejectedTinyContourCount
                };

                foreach (var warning in warnings)
                    result.Warnings.Add(warning);
            }

            if (!string.IsNullOrWhiteSpace(standardError))
                result.Warnings.Add(standardError.Trim());

            return result;
        }
        catch (Exception ex)
        {
            var fallback = _fallbackTraceService.Trace(image, settings);
            fallback.TraceEngine = "Legacy contour tracer";
            fallback.Warnings.Add($"Potrace tracing failed, so bitmap tracing fell back to the legacy contour tracer. {ex.Message}");
            return fallback;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }

    private string? ResolvePotracePath()
    {
        if (!string.IsNullOrWhiteSpace(_potracePath))
            return _potracePath;

        var candidateRoots = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Tracing", "Potrace"),
            Path.Combine(AppContext.BaseDirectory, "tools", "potrace"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tools", "potrace"))
        };

        foreach (var candidateRoot in candidateRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(candidateRoot))
                continue;

            var executable = Directory.GetFiles(candidateRoot, "potrace.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(executable))
                return executable;
        }

        return null;
    }

    private static void WritePortableBitmap(bool[,] mask, int width, int height, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.ASCII);
        writer.WriteLine("P1");
        writer.WriteLine($"{width} {height}");

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                writer.Write(mask[y, x] ? "1" : "0");
                writer.Write(' ');
            }

            writer.WriteLine();
        }
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

        var grayscalePixels = new byte[height, width];
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

                grayscalePixels[y, x] = alpha == 0 ? (byte)255 : (byte)grayscale;
            }
        }

        var thresholdMask = settings.UseAdaptiveThreshold
            ? AdaptiveThreshold(grayscalePixels, settings.Threshold)
            : FixedThreshold(grayscalePixels, settings.Threshold);

        if (settings.Invert)
            thresholdMask = Invert(thresholdMask);

        var mask = (bool[,])thresholdMask.Clone();
        if (settings.MorphologyOpenIterations > 0)
            mask = MorphologyOpen(mask, settings.MorphologyOpenIterations);
        if (settings.MorphologyCloseIterations > 0)
            mask = MorphologyClose(mask, settings.MorphologyCloseIterations);
        if (settings.ErosionIterations > 0)
            mask = Erode(mask, settings.ErosionIterations);
        if (settings.DilationIterations > 0)
            mask = Dilate(mask, settings.DilationIterations);
        if (settings.Despeckle)
            mask = Despeckle(mask, settings);

        return new RasterTraceImage
        {
            Mask = mask,
            ThresholdMask = thresholdMask,
            CleanedMask = mask,
            WidthPixels = width,
            HeightPixels = height
        };
    }

    private static bool[,] FixedThreshold(byte[,] grayscalePixels, int threshold)
    {
        var height = grayscalePixels.GetLength(0);
        var width = grayscalePixels.GetLength(1);
        var mask = new bool[height, width];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                mask[y, x] = grayscalePixels[y, x] <= threshold;
        }

        return mask;
    }

    private static bool[,] AdaptiveThreshold(byte[,] grayscalePixels, int thresholdBias)
    {
        var height = grayscalePixels.GetLength(0);
        var width = grayscalePixels.GetLength(1);
        var mask = new bool[height, width];
        const int radius = 4;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sum = 0;
                var count = 0;
                for (var oy = -radius; oy <= radius; oy++)
                {
                    for (var ox = -radius; ox <= radius; ox++)
                    {
                        var nx = x + ox;
                        var ny = y + oy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        sum += grayscalePixels[ny, nx];
                        count++;
                    }
                }

                var localAverage = count == 0 ? thresholdBias : sum / count;
                var adjustedThreshold = Math.Clamp(localAverage - (255 - thresholdBias) / 6, 20, 235);
                mask[y, x] = grayscalePixels[y, x] <= adjustedThreshold;
            }
        }

        return mask;
    }

    private static bool[,] Invert(bool[,] source)
    {
        var height = source.GetLength(0);
        var width = source.GetLength(1);
        var result = new bool[height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                result[y, x] = !source[y, x];
        }

        return result;
    }

    private static bool[,] MorphologyOpen(bool[,] source, int iterations)
        => Dilate(Erode(source, iterations), iterations);

    private static bool[,] MorphologyClose(bool[,] source, int iterations)
        => Erode(Dilate(source, iterations), iterations);

    private static bool[,] Erode(bool[,] source, int iterations)
    {
        var current = (bool[,])source.Clone();
        for (var i = 0; i < iterations; i++)
            current = ApplyMorphology(current, requireAllNeighbors: true);
        return current;
    }

    private static bool[,] Dilate(bool[,] source, int iterations)
    {
        var current = (bool[,])source.Clone();
        for (var i = 0; i < iterations; i++)
            current = ApplyMorphology(current, requireAllNeighbors: false);
        return current;
    }

    private static bool[,] ApplyMorphology(bool[,] source, bool requireAllNeighbors)
    {
        var height = source.GetLength(0);
        var width = source.GetLength(1);
        var result = new bool[height, width];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var hits = 0;
                var total = 0;
                for (var oy = -1; oy <= 1; oy++)
                {
                    for (var ox = -1; ox <= 1; ox++)
                    {
                        var nx = x + ox;
                        var ny = y + oy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        total++;
                        if (source[ny, nx])
                            hits++;
                    }
                }

                result[y, x] = requireAllNeighbors ? hits == total : hits > 0;
            }
        }

        return result;
    }

    private static bool[,] Despeckle(bool[,] source, ImageToolpathSettings settings)
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

        var minimumComponentSize = Math.Max(4, (int)Math.Round((double)settings.MinimumFeatureSizeMm * 4d));
        return RemoveSmallComponents(clone, minimumComponentSize);
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
            return new VectorTraceResult { TraceEngine = "Legacy contour tracer" };

        var scale = ResolveScale(settings, bounds.Width, bounds.Height);
        var tolerancePx = ResolvePixels(settings.SimplifyToleranceMm, scale, fallback: 0.5);
        var minSegmentPx = ResolvePixels(settings.MinimumSegmentLengthMm, scale, fallback: 0.25);
        var closeGapPx = ResolvePixels(settings.CloseGapToleranceMm, scale, fallback: 0.75);
        var minAreaPx = ResolveSquarePixels(settings.MinimumContourAreaMm2, scale, fallback: 3d);
        var minIslandPx = ResolveSquarePixels(settings.MinimumIslandSizeMm2, scale, fallback: minAreaPx);
        var minPerimeterPx = ResolvePixels(settings.MinimumContourPerimeterMm, scale, fallback: 2d);
        var output = new List<VectorPath>();
        var removedPathCount = 0;
        var segmentReduction = 0;
        var circleFitCount = 0;
        var arcFitCount = 0;
        var mergedSegments = 0;
        var simplifiedPoints = 0;
        var rejectedTinyContours = 0;
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

            var beforeMergeCount = cleaned.Count;
            cleaned = SmoothPath(cleaned, settings.SmoothingAmount);
            cleaned = MergeCollinear(cleaned, minSegmentPx);
            mergedSegments += Math.Max(0, beforeMergeCount - cleaned.Count);

            var originalCount = cleaned.Count;
            var simplified = Simplify(cleaned, tolerancePx);
            simplified = MergeCollinear(simplified, minSegmentPx);
            simplified = CleanupRawPath(simplified, closeGapPx, minSegmentPx);
            if (simplified.Count < 5)
            {
                removedPathCount++;
                continue;
            }

            segmentReduction += Math.Max(0, originalCount - simplified.Count);
            simplifiedPoints += Math.Max(0, beforeMergeCount - simplified.Count);

            var signedArea = SignedArea(simplified);
            var absoluteArea = Math.Abs(signedArea);
            var closed = Distance(simplified[0], simplified[^1]) < closeGapPx;
            var perimeter = Perimeter(simplified);
            if (closed && (absoluteArea < minAreaPx || perimeter < minPerimeterPx))
            {
                removedPathCount++;
                rejectedTinyContours++;
                continue;
            }

            var vectorPath = new VectorPath
            {
                SourceIndex = rawIndex,
                Closed = closed
            };

            foreach (var point in simplified)
            {
                vectorPath.Points.Add(new VectorPoint
                {
                    X = (point.X - bounds.MinX) * scale,
                    Y = (point.Y - bounds.MinY) * scale
                });
            }

            vectorPath.SignedArea = SignedArea(vectorPath.Points.Select(point => (point.X, point.Y)).ToList());
            vectorPath.AbsoluteArea = Math.Abs(vectorPath.SignedArea);
            UpdateMetrics(vectorPath);
            BuildSegments(vectorPath);
            output.Add(vectorPath);
        }

        var joinedPathCount = 0;
        var lineFitCount = 0;
        output = JoinOpenStrokes(output, (double)settings.CloseGapToleranceMm, ref joinedPathCount);
        ClassifyHierarchy(output);
        if (settings.TraceMode == ImageTraceMode.TechnicalDrawing)
            output = FilterTechnicalDrawingContours(output, warnings);

        foreach (var path in output)
        {
            if (TryCollapseLine(path))
                lineFitCount++;

            DetectCircle(path);
            if (path.IsCircleLike)
            {
                circleFitCount++;
                continue;
            }

            DetectArc(path);
            if (path.IsArcLike)
                arcFitCount++;
        }

        var beforeIslandReject = output.Count;
        output = output
            .Where(path => path.Kind != VectorPathKind.Island || path.AbsoluteArea >= (double)settings.MinimumIslandSizeMm2)
            .ToList();
        rejectedTinyContours += Math.Max(0, beforeIslandReject - output.Count);
        var ordered = OrderPaths(output);
        for (var index = 0; index < ordered.Count; index++)
            ordered[index].OrderIndex = index + 1;

        if (settings.ManufacturingMode is ImageManufacturingMode.ProfileInside or ImageManufacturingMode.ProfileOutside)
            warnings.Add($"{settings.ManufacturingMode} is selected, but real tool diameter compensation is not implemented yet. Paths are currently generated on-line.");

        var traceResult = new VectorTraceResult
        {
            TraceEngine = "Legacy contour tracer",
            Paths = ordered,
            RemovedPathCount = removedPathCount,
            JoinedPathCount = joinedPathCount,
            LineFitCount = lineFitCount,
            ArcFitCount = arcFitCount,
            CircleFitCount = circleFitCount,
            SegmentReductionCount = segmentReduction,
            RemovedArtifactCount = removedPathCount,
            MergedSegmentCount = mergedSegments,
            SimplifiedPointCount = simplifiedPoints,
            RejectedTinyContourCount = rejectedTinyContours
        };

        foreach (var warning in warnings)
            traceResult.Warnings.Add(warning);

        return traceResult;
    }

    internal static List<VectorPath> FilterTechnicalDrawingContours(List<VectorPath> paths, List<string> warnings)
    {
        var closedContours = paths.Where(path => path.Closed).ToList();
        if (closedContours.Count == 0)
        {
            warnings.Add("Technical Drawing mode found no closed contours, so open strokes were not promoted to cut geometry.");
            return paths.Where(path => path.Closed).ToList();
        }

        var largestOuterArea = closedContours
            .Where(path => path.Kind == VectorPathKind.OuterContour)
            .Select(path => path.AbsoluteArea)
            .DefaultIfEmpty(closedContours.Max(path => path.AbsoluteArea))
            .Max();
        var maxPerimeter = closedContours
            .Select(path => Perimeter(path.Points.Select(point => (point.X, point.Y)).ToList()))
            .DefaultIfEmpty(0d)
            .Max();

        var rootCandidates = closedContours
            .Where(path => path.Kind == VectorPathKind.OuterContour)
            .Where(path =>
                path.AbsoluteArea >= largestOuterArea * 0.08d ||
                Perimeter(path.Points.Select(point => (point.X, point.Y)).ToList()) >= maxPerimeter * 0.35d)
            .ToList();

        if (rootCandidates.Count == 0)
            rootCandidates = closedContours
                .OrderByDescending(path => path.AbsoluteArea)
                .Take(1)
                .ToList();

        var keep = new HashSet<VectorPath>();
        foreach (var root in rootCandidates)
        {
            keep.Add(root);
            var descendants = closedContours
                .Where(candidate => candidate != root && ContainsPoint(root, candidate.CentroidX, candidate.CentroidY))
                .Where(candidate => candidate.AbsoluteArea >= root.AbsoluteArea * 0.01d)
                .ToList();

            foreach (var descendant in descendants)
                keep.Add(descendant);
        }

        var filtered = closedContours.Where(keep.Contains).ToList();
        var removedOpen = paths.Count(path => !path.Closed);
        var removedClosed = closedContours.Count - filtered.Count;
        if (removedOpen > 0 || removedClosed > 0)
            warnings.Add($"Technical Drawing mode filtered {removedOpen} open annotation path(s) and {removedClosed} small closed annotation contour(s).");

        ClassifyHierarchy(filtered);
        return filtered;
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

    internal static List<(double X, double Y)> CleanupRawPath(
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

    internal static List<(double X, double Y)> SmoothPath(List<(double X, double Y)> points, decimal smoothingAmount)
    {
        if (points.Count < 5 || smoothingAmount <= 0m)
            return points;

        var factor = Math.Clamp((double)smoothingAmount, 0d, 1d) * 0.45d;
        var smoothed = new List<(double X, double Y)>(points.Count) { points[0] };
        for (var i = 1; i < points.Count - 1; i++)
        {
            var previous = points[i - 1];
            var current = points[i];
            var next = points[i + 1];
            var averageX = (previous.X + current.X + next.X) / 3d;
            var averageY = (previous.Y + current.Y + next.Y) / 3d;
            smoothed.Add((
                current.X + ((averageX - current.X) * factor),
                current.Y + ((averageY - current.Y) * factor)));
        }

        smoothed.Add(points[^1]);
        return smoothed;
    }

    internal static List<(double X, double Y)> MergeCollinear(List<(double X, double Y)> points, double minimumSegmentLength)
    {
        if (points.Count < 4)
            return points;

        var merged = new List<(double X, double Y)> { points[0] };
        for (var i = 1; i < points.Count - 1; i++)
        {
            var previous = merged[^1];
            var current = points[i];
            var next = points[i + 1];

            if (Distance(previous, current) < minimumSegmentLength)
                continue;

            var cross = Math.Abs(((current.X - previous.X) * (next.Y - current.Y)) - ((current.Y - previous.Y) * (next.X - current.X)));
            var span = Math.Max(minimumSegmentLength, Distance(previous, next));
            if (cross / span < 0.02d)
                continue;

            merged.Add(current);
        }

        merged.Add(points[^1]);
        return merged;
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

    internal static List<(double X, double Y)> Simplify(List<(double X, double Y)> points, double tolerance)
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

    internal static double Distance((double X, double Y) a, (double X, double Y) b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static double Perimeter(IReadOnlyList<(double X, double Y)> points)
    {
        var total = 0d;
        for (var i = 1; i < points.Count; i++)
            total += Distance(points[i - 1], points[i]);

        return total;
    }

    internal static double SignedArea(IReadOnlyList<(double X, double Y)> points)
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

    internal static void UpdateMetrics(VectorPath path)
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

    internal static void BuildSegments(VectorPath path)
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

    internal static void DetectCircle(VectorPath path)
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

    internal static void DetectArc(VectorPath path)
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

    internal static void ClassifyHierarchy(List<VectorPath> paths)
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

    internal static List<VectorPath> OrderPaths(List<VectorPath> paths)
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

    internal static List<VectorPath> JoinOpenStrokes(List<VectorPath> paths, double tolerance, ref int joinedPathCount)
    {
        var closed = paths.Where(path => path.Closed).ToList();
        var open = paths.Where(path => !path.Closed && path.Points.Count >= 2).ToList();

        var changed = true;
        while (changed)
        {
            changed = false;
            for (var i = 0; i < open.Count && !changed; i++)
            {
                for (var j = i + 1; j < open.Count && !changed; j++)
                {
                    if (!TryJoinPaths(open[i], open[j], tolerance, out var joined))
                        continue;

                    open[i] = joined;
                    open.RemoveAt(j);
                    joinedPathCount++;
                    changed = true;
                }
            }
        }

        var result = new List<VectorPath>(closed.Count + open.Count);
        result.AddRange(closed);
        result.AddRange(open);
        for (var index = 0; index < result.Count; index++)
            result[index].SourceIndex = index;
        return result;
    }

    internal static bool TryCollapseLine(VectorPath path)
    {
        if (path.Closed || path.Points.Count <= 2)
            return false;

        var start = path.Points.First();
        var end = path.Points.Last();
        var startTuple = (start.X, start.Y);
        var endTuple = (end.X, end.Y);
        var maxDeviation = path.Points
            .Skip(1)
            .Take(Math.Max(0, path.Points.Count - 2))
            .Select(point => PerpendicularDistance((point.X, point.Y), startTuple, endTuple))
            .DefaultIfEmpty(0d)
            .Max();

        var span = Distance(startTuple, endTuple);
        if (span <= 0.0001 || maxDeviation > Math.Max(span * 0.02d, 0.15d))
            return false;

        path.Points.Clear();
        path.Points.Add(new VectorPoint { X = start.X, Y = start.Y });
        path.Points.Add(new VectorPoint { X = end.X, Y = end.Y });
        BuildSegments(path);
        UpdateMetrics(path);
        return true;
    }

    private static bool TryJoinPaths(VectorPath first, VectorPath second, double tolerance, out VectorPath joined)
    {
        joined = new VectorPath();
        var firstStart = first.Points.First();
        var firstEnd = first.Points.Last();
        var secondStart = second.Points.First();
        var secondEnd = second.Points.Last();

        List<VectorPoint>? combined = null;
        if (Distance((firstEnd.X, firstEnd.Y), (secondStart.X, secondStart.Y)) <= tolerance)
            combined = first.Points.Concat(second.Points.Skip(1)).Select(ClonePoint).ToList();
        else if (Distance((firstEnd.X, firstEnd.Y), (secondEnd.X, secondEnd.Y)) <= tolerance)
            combined = first.Points.Concat(second.Points.Reverse().Skip(1)).Select(ClonePoint).ToList();
        else if (Distance((firstStart.X, firstStart.Y), (secondEnd.X, secondEnd.Y)) <= tolerance)
            combined = second.Points.Concat(first.Points.Skip(1)).Select(ClonePoint).ToList();
        else if (Distance((firstStart.X, firstStart.Y), (secondStart.X, secondStart.Y)) <= tolerance)
            combined = second.Points.Reverse().Concat(first.Points.Skip(1)).Select(ClonePoint).ToList();

        if (combined == null)
            return false;

        foreach (var point in combined)
            joined.Points.Add(point);
        joined.Closed = false;
        joined.Kind = VectorPathKind.OpenStroke;
        joined.SignedArea = 0d;
        joined.AbsoluteArea = 0d;
        BuildSegments(joined);
        UpdateMetrics(joined);
        return true;
    }

    private static VectorPoint ClonePoint(VectorPoint point) => new() { X = point.X, Y = point.Y };

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

        var originalWarnings = traceResult.Warnings.ToList();
        var normalizedPaths = NormalizePathsToLocalOrigin(traceResult.Paths);
        traceResult = new VectorTraceResult
        {
            TraceEngine = traceResult.TraceEngine,
            Paths = normalizedPaths,
            RemovedPathCount = traceResult.RemovedPathCount,
            JoinedPathCount = traceResult.JoinedPathCount,
            LineFitCount = traceResult.LineFitCount,
            ArcFitCount = traceResult.ArcFitCount,
            CircleFitCount = traceResult.CircleFitCount,
            SegmentReductionCount = traceResult.SegmentReductionCount,
            RemovedArtifactCount = traceResult.RemovedArtifactCount,
            MergedSegmentCount = traceResult.MergedSegmentCount,
            SimplifiedPointCount = traceResult.SimplifiedPointCount,
            RejectedTinyContourCount = traceResult.RejectedTinyContourCount
        };
        foreach (var warning in originalWarnings)
            traceResult.Warnings.Add(warning);

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

    private static IReadOnlyList<VectorPath> NormalizePathsToLocalOrigin(IReadOnlyList<VectorPath> paths)
    {
        if (paths.Count == 0)
            return paths;

        var minX = paths.Min(path => path.MinX);
        var minY = paths.Min(path => path.MinY);
        if (Math.Abs(minX) < 0.0001 && Math.Abs(minY) < 0.0001)
            return paths;

        var normalized = new List<VectorPath>(paths.Count);
        foreach (var path in paths)
        {
            var clone = new VectorPath
            {
                SourceIndex = path.SourceIndex,
                Closed = path.Closed,
                Kind = path.Kind,
                OrderIndex = path.OrderIndex,
                NestingDepth = path.NestingDepth,
                ParentPathIndex = path.ParentPathIndex,
                IsCircleLike = path.IsCircleLike,
                CircleCenterX = path.CircleCenterX.HasValue ? path.CircleCenterX.Value - minX : null,
                CircleCenterY = path.CircleCenterY.HasValue ? path.CircleCenterY.Value - minY : null,
                CircleRadius = path.CircleRadius,
                IsArcLike = path.IsArcLike,
                ArcCenterX = path.ArcCenterX.HasValue ? path.ArcCenterX.Value - minX : null,
                ArcCenterY = path.ArcCenterY.HasValue ? path.ArcCenterY.Value - minY : null,
                ArcRadius = path.ArcRadius,
                ArcClockwise = path.ArcClockwise
            };

            foreach (var point in path.Points)
            {
                clone.Points.Add(new VectorPoint
                {
                    X = point.X - minX,
                    Y = point.Y - minY
                });
            }

            clone.SignedArea = path.SignedArea;
            clone.AbsoluteArea = path.AbsoluteArea;
            OutlineVectorTraceService.UpdateMetrics(clone);
            OutlineVectorTraceService.BuildSegments(clone);
            normalized.Add(clone);
        }

        return normalized;
    }

    private static ImageToolpathDiagnostics BuildDiagnostics(VectorTraceResult traceResult, GeneratedGcodeResult gcode)
    {
        var diagnostics = new ImageToolpathDiagnostics
        {
            TraceEngine = traceResult.TraceEngine,
            TracedPathCount = traceResult.Paths.Count,
            RemovedPathCount = traceResult.RemovedPathCount,
            JoinedPathCount = traceResult.JoinedPathCount,
            ClosedContourCount = traceResult.Paths.Count(path => path.Closed),
            OpenStrokeCount = traceResult.Paths.Count(path => path.Kind == VectorPathKind.OpenStroke),
            HoleCount = traceResult.Paths.Count(path => path.Kind == VectorPathKind.InnerHole),
            LineFitCount = traceResult.LineFitCount,
            ArcFitCount = traceResult.ArcFitCount,
            CircleFitCount = traceResult.CircleFitCount,
            SegmentReductionCount = traceResult.SegmentReductionCount,
            RemovedArtifactCount = traceResult.RemovedArtifactCount,
            MergedSegmentCount = traceResult.MergedSegmentCount,
            SimplifiedPointCount = traceResult.SimplifiedPointCount,
            RejectedTinyContourCount = traceResult.RejectedTinyContourCount,
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
        var tracedBuilder = new StringBuilder();
        var minX = paths.Min(path => path.MinX);
        var minY = paths.Min(path => path.MinY);
        var maxX = paths.Max(path => path.MaxX);
        var maxY = paths.Max(path => path.MaxY);
        preview.BoundingBoxGeometryData = $"M {minX:0.###},{minY:0.###} L {maxX:0.###},{minY:0.###} L {maxX:0.###},{maxY:0.###} L {minX:0.###},{maxY:0.###} Z";

        VectorPoint? previousEnd = null;
        foreach (var path in paths.Where(path => path.Points.Count > 1))
        {
            var first = path.Points[0];
            tracedBuilder.Append($"M {first.X:0.###},{first.Y:0.###} ");
            foreach (var tracePoint in path.Points.Skip(1))
                tracedBuilder.Append($"L {tracePoint.X:0.###},{tracePoint.Y:0.###} ");
            if (path.Closed)
                tracedBuilder.Append("Z ");

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

        preview.TracedGeometryData = tracedBuilder.ToString().Trim();
        preview.CutGeometryData = cutBuilder.ToString().Trim();
        preview.RapidGeometryData = rapidBuilder.ToString().Trim();
        return preview;
    }
}

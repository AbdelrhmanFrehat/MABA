using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IGcodeParserService
{
    GcodeParseResult ParseFile(string filePath);
    GcodeParseResult ParseText(string fileName, string contents, string? sourcePath = null);
}

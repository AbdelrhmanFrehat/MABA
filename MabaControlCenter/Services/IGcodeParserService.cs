using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface IGcodeParserService
{
    GcodeParseResult ParseFile(string filePath);
}

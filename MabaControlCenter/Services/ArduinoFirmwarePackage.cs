using System.IO;
using System.Text.RegularExpressions;

namespace MabaControlCenter.Services;

public static class ArduinoFirmwarePackage
{
    public const string BundledFileName = "MabaArduinoCncFirmware.ino";
    private static readonly Regex VersionRegex = new(@"MABA_FIRMWARE_VERSION:\s*(?<version>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BoardRegex = new(@"TARGET_BOARD:\s*(?<board>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string BundledFirmwarePath =>
        Path.Combine(AppContext.BaseDirectory, "Firmware", BundledFileName);

    public static bool Exists() => File.Exists(BundledFirmwarePath);

    public static string FirmwareVersion => ReadMetadata(VersionRegex, "2.1.2");

    public static string TargetBoardDisplay => ReadMetadata(BoardRegex, "Arduino Uno (arduino:avr:uno)");

    public static void ExportTo(string destinationPath)
    {
        if (!Exists())
            throw new FileNotFoundException("Bundled Arduino firmware package was not found.", BundledFirmwarePath);

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        File.Copy(BundledFirmwarePath, destinationPath, overwrite: true);
    }

    private static string ReadMetadata(Regex regex, string fallback)
    {
        if (!Exists())
            return fallback;

        try
        {
            var text = File.ReadAllText(BundledFirmwarePath);
            var match = regex.Match(text);
            if (!match.Success)
                return fallback;

            var value = match.Groups["version"].Success
                ? match.Groups["version"].Value.Trim()
                : match.Groups["board"].Success
                    ? match.Groups["board"].Value.Trim()
                    : string.Empty;

            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
        catch
        {
            return fallback;
        }
    }
}

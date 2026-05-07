using System.IO;

namespace MabaControlCenter.Services;

public static class ArduinoFirmwarePackage
{
    public const string BundledFileName = "MabaArduinoCncFirmware.ino";

    public static string BundledFirmwarePath =>
        Path.Combine(AppContext.BaseDirectory, "Firmware", BundledFileName);

    public static bool Exists() => File.Exists(BundledFirmwarePath);

    public static void ExportTo(string destinationPath)
    {
        if (!Exists())
            throw new FileNotFoundException("Bundled Arduino firmware package was not found.", BundledFirmwarePath);

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        File.Copy(BundledFirmwarePath, destinationPath, overwrite: true);
    }
}

using System.Diagnostics;
using System.IO;
using System.Text;

namespace MabaControlCenter.Services;

public static class ArduinoFirmwareUploader
{
    private const string BoardFqbn = "arduino:avr:uno";

    public static string ToolingStatus
    {
        get
        {
            var cli = TryFindArduinoCli();
            return cli != null
                ? $"Arduino CLI detected: {cli}"
                : "Arduino CLI not found. Install Arduino IDE / Arduino CLI on this PC to enable one-click firmware upload.";
        }
    }

    public static bool CanUpload => TryFindArduinoCli() != null && ArduinoFirmwarePackage.Exists();

    public static async Task<ArduinoFirmwareUploadResult> UploadBundledFirmwareAsync(string portName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(portName))
            return ArduinoFirmwareUploadResult.CreateFailure("Select a hardware COM port before uploading firmware.");

        if (!ArduinoFirmwarePackage.Exists())
            return ArduinoFirmwareUploadResult.CreateFailure("The bundled Arduino firmware package is missing from this app build.");

        var cliPath = TryFindArduinoCli();
        if (cliPath == null)
            return ArduinoFirmwareUploadResult.CreateFailure("Arduino CLI was not found on this PC. Install Arduino IDE / Arduino CLI first.");

        var tempRoot = Path.Combine(Path.GetTempPath(), "MabaControlCenter", "arduino-fw", Guid.NewGuid().ToString("N"));
        var sketchDir = Path.Combine(tempRoot, "MabaArduinoCncFirmware");
        var sketchPath = Path.Combine(sketchDir, ArduinoFirmwarePackage.BundledFileName);
        Directory.CreateDirectory(sketchDir);
        File.Copy(ArduinoFirmwarePackage.BundledFirmwarePath, sketchPath, overwrite: true);

        try
        {
            var compileArgs = $"compile --fqbn {BoardFqbn} \"{sketchDir}\"";
            var compile = await RunProcessAsync(cliPath, compileArgs, tempRoot, cancellationToken).ConfigureAwait(false);
            if (compile.ExitCode != 0)
                return ArduinoFirmwareUploadResult.CreateFailure("Firmware compile failed.", compile.Output);

            var uploadArgs = $"upload -p {portName} --fqbn {BoardFqbn} \"{sketchDir}\"";
            var upload = await RunProcessAsync(cliPath, uploadArgs, tempRoot, cancellationToken).ConfigureAwait(false);
            if (upload.ExitCode != 0)
                return ArduinoFirmwareUploadResult.CreateFailure("Firmware upload failed.", upload.Output);

            return ArduinoFirmwareUploadResult.CreateSuccess(
                ArduinoFirmwarePackage.FirmwareVersion,
                ArduinoFirmwarePackage.TargetBoardDisplay,
                upload.Output);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, recursive: true);
            }
            catch
            {
                // Ignore temp cleanup failures.
            }
        }
    }

    private static async Task<ProcessRunResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                output.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        return new ProcessRunResult(process.ExitCode, output.ToString().Trim());
    }

    private static string? TryFindArduinoCli()
    {
        var candidates = new List<string>();
        var pathExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Arduino CLI", "arduino-cli.exe");
        candidates.Add(pathExe);
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Arduino CLI", "arduino-cli.exe"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Arduino CLI", "arduino-cli.exe"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Arduino IDE", "resources", "app", "lib", "backend", "resources", "arduino-cli.exe"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Arduino IDE", "resources", "app", "lib", "backend", "resources", "arduino-cli.exe"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Arduino", "arduino-cli.exe"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Arduino", "arduino-cli.exe"));

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var segment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = Path.Combine(segment, "arduino-cli.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private sealed record ProcessRunResult(int ExitCode, string Output);
}

public sealed class ArduinoFirmwareUploadResult
{
    public bool Success { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public string Output { get; private init; } = string.Empty;
    public string FirmwareVersion { get; private init; } = string.Empty;
    public string TargetBoard { get; private init; } = string.Empty;

    public static ArduinoFirmwareUploadResult CreateSuccess(string firmwareVersion, string targetBoard, string output) => new()
    {
        Success = true,
        Message = "Firmware uploaded successfully.",
        Output = output,
        FirmwareVersion = firmwareVersion,
        TargetBoard = targetBoard
    };

    public static ArduinoFirmwareUploadResult CreateFailure(string message, string? output = null) => new()
    {
        Success = false,
        Message = message,
        Output = output ?? string.Empty
    };
}

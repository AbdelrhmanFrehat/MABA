using System.Diagnostics;
using System.IO;

namespace MabaUpdater;

internal sealed class UpdateInstallService
{
    public async Task RunAsync(
        UpdaterArguments args,
        Action<string, string, bool, double?> report,
        CancellationToken cancellationToken)
    {
        report("Preparing", BuildSubtitle(args), true, null);
        await WaitForProcessExitAsync(args.ProcessId, report, cancellationToken);

        report("Verifying package", "Checking update files before install.", true, null);
        if (!Directory.Exists(args.SourceDirectory))
            throw new DirectoryNotFoundException($"Update source folder not found: {args.SourceDirectory}");

        var plan = BuildPlan(args.SourceDirectory, args.TargetDirectory);
        var totalOperations = Math.Max(1, plan.CreateDirectories.Count + plan.CopyFiles.Count + plan.DeleteFiles.Count + plan.DeleteDirectories.Count);
        var completed = 0;

        foreach (var directory in plan.CreateDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(directory);
            completed++;
            report("Installing update", $"Preparing folder {Path.GetFileName(directory)}", false, (double)completed / totalOperations);
        }

        foreach (var file in plan.CopyFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(file.TargetPath)!);
            File.Copy(file.SourcePath, file.TargetPath, overwrite: true);
            completed++;
            report("Installing update", $"Copying {Path.GetFileName(file.TargetPath)}", false, (double)completed / totalOperations);
        }

        foreach (var file in plan.DeleteFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(file))
                File.Delete(file);

            completed++;
            report("Cleaning old files", $"Removing {Path.GetFileName(file)}", false, (double)completed / totalOperations);
        }

        foreach (var directory in plan.DeleteDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);

            completed++;
            report("Cleaning old files", $"Removing {Path.GetFileName(directory)}", false, (double)completed / totalOperations);
        }

        report("Restarting", "Launching the updated app.", true, null);
        StartRelaunch(args.RelaunchPath);
    }

    private static async Task WaitForProcessExitAsync(
        int pid,
        Action<string, string, bool, double?> report,
        CancellationToken cancellationToken)
    {
        report("Closing app", "Waiting for Maba Control Center to shut down cleanly.", true, null);

        try
        {
            using var process = Process.GetProcessById(pid);
            while (!process.HasExited)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(300, cancellationToken);
                process.Refresh();
            }
        }
        catch (ArgumentException)
        {
            // Already exited.
        }
    }

    private static UpdateInstallPlan BuildPlan(string sourceDirectory, string targetDirectory)
    {
        var sourceDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(sourceDirectory, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sourceFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories)
            .Select(path => new PlannedCopy(
                path,
                Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, path))))
            .OrderBy(path => path.TargetPath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var targetDirectories = Directory.Exists(targetDirectory)
            ? Directory.GetDirectories(targetDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(targetDirectory, path))
                .ToList()
            : new List<string>();

        var targetFiles = Directory.Exists(targetDirectory)
            ? Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(targetDirectory, path))
                .ToList()
            : new List<string>();

        var sourceDirSet = sourceDirectories.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceFileSet = sourceFiles
            .Select(x => Path.GetRelativePath(targetDirectory, x.TargetPath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var createDirectories = sourceDirectories
            .Select(path => Path.Combine(targetDirectory, path))
            .ToList();

        var deleteFiles = targetFiles
            .Where(relative => !sourceFileSet.Contains(relative))
            .Select(relative => Path.Combine(targetDirectory, relative))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var deleteDirectories = targetDirectories
            .Where(relative => !sourceDirSet.Contains(relative))
            .OrderByDescending(path => path.Length)
            .Select(relative => Path.Combine(targetDirectory, relative))
            .ToList();

        return new UpdateInstallPlan(createDirectories, sourceFiles, deleteFiles, deleteDirectories);
    }

    private static string BuildSubtitle(UpdaterArguments args)
    {
        return string.IsNullOrWhiteSpace(args.Version)
            ? "Preparing the next MABA Control Center build."
            : $"Preparing Maba Control Center {args.Version}.";
    }

    private static void StartRelaunch(string relaunchPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = relaunchPath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(relaunchPath) ?? AppContext.BaseDirectory
        };
        Process.Start(startInfo);
    }

    private sealed record PlannedCopy(string SourcePath, string TargetPath);

    private sealed record UpdateInstallPlan(
        List<string> CreateDirectories,
        List<PlannedCopy> CopyFiles,
        List<string> DeleteFiles,
        List<string> DeleteDirectories);
}

namespace Kiji.Templates.Tests;

internal sealed class TestRunDirectory : IDisposable
{
    private readonly string resultsDirectory;
    private readonly FileStream activeRun;
    private readonly HashSet<string> projects = [];

    internal string Root { get; }

    internal TestRunDirectory(string resultsDirectory)
    {
        this.resultsDirectory = Path.GetFullPath(resultsDirectory);
        Root = Directory.CreateDirectory(Path.Combine(this.resultsDirectory, $"tests-{Guid.NewGuid():N}")).FullName;
        activeRun = File.Open(Path.Combine(Root, ".run.lock"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        Prune();
    }

    internal string ProjectDirectory(string kind, string scenario)
    {
        var project = Path.GetFullPath(Path.Combine(Root, "sites", kind, scenario, "MySite"));
        if (!project.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException($"Project is outside the test run: {project}");
        projects.Add(project);
        return project;
    }

    public void Dispose()
    {
        try
        {
            foreach (var project in projects)
                foreach (var name in new[] { "bin", "obj" })
                    Cleanup(() =>
                    {
                        var directory = Path.Combine(project, name);
                        if (Directory.Exists(directory))
                            Directory.Delete(directory, recursive: true);
                    });
        }
        finally
        {
            activeRun.Dispose();
            Prune();
        }
    }

    private void Prune() => Cleanup(() =>
    {
        var root = new DirectoryInfo(resultsDirectory);
        var runs = root.EnumerateDirectories("tests-*")
            .Where(directory => Guid.TryParseExact(directory.Name[6..], "N", out _));
        foreach (var group in runs.Cast<FileSystemInfo>().Concat(root.EnumerateFiles("*.trx"))
            .Where(entry => !entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
            .GroupBy(entry => entry is DirectoryInfo))
        {
            var kept = 0;
            foreach (var entry in group.OrderByDescending(entry => entry.LastWriteTimeUtc))
            {
                FileStream lease;
                try
                {
                    var path = entry is DirectoryInfo ? Path.Combine(entry.FullName, ".run.lock") : entry.FullName;
                    lease = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
                }
                catch (IOException) { continue; } // Active or already removed by another run.
                using (lease)
                {
                    if (kept++ < 5)
                        continue;
                    Cleanup(() =>
                    {
                        if (entry is DirectoryInfo directory) directory.Delete(recursive: true);
                        else entry.Delete();
                    });
                }
            }
        }
    });

    private static void Cleanup(Action action)
    {
        try { action(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Test artifact cleanup: {exception.Message}");
        }
    }
}

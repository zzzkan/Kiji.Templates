using Xunit;

namespace Kiji.Templates.Tests;

public sealed class TestRunDirectoryTests : IDisposable
{
    private readonly string root = Directory.CreateDirectory(Path.Combine(
        TemplateFixture.Repository, "TestResults", $"retention-check-{Guid.NewGuid():N}")).FullName;

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public void DisposalRemovesBuildOutputsAndKeepsDiagnostics()
    {
        string project, diagnostic;
        using (var run = new TestRunDirectory(root))
        {
            project = run.ProjectDirectory("minimal", "root");
            foreach (var name in new[] { "bin", "obj", "dist" })
                Write(Path.Combine(project, name, "output.txt"));
            Write(Path.Combine(project, "Program.cs"));
            diagnostic = Path.Combine(run.Root, "logs", "failure.png");
            Write(diagnostic);
        }

        Assert.False(Directory.Exists(Path.Combine(project, "bin")));
        Assert.False(Directory.Exists(Path.Combine(project, "obj")));
        Assert.True(File.Exists(Path.Combine(project, "dist", "output.txt")));
        Assert.True(File.Exists(Path.Combine(project, "Program.cs")));
        Assert.True(File.Exists(diagnostic));
    }

    [Fact]
    public void RetentionKeepsFiveRecentResultsAndProtectsActiveRuns()
    {
        using var active = new TestRunDirectory(root);
        Directory.SetLastWriteTimeUtc(active.Root, DateTime.UtcNow.AddHours(-2));
        var notes = Path.Combine(root, "notes", "keep.txt");
        Write(notes);
        var runs = new List<string>();
        for (var index = 0; index < 6; index++)
        {
            var report = Path.Combine(root, $"result-{index}.trx");
            Write(report);
            File.SetLastWriteTimeUtc(report, DateTime.UtcNow.AddHours(-1).AddMinutes(index));
            using var run = new TestRunDirectory(root);
            Write(Path.Combine(run.Root, "logs", "diagnostic.txt"));
            Directory.SetLastWriteTimeUtc(run.Root, DateTime.UtcNow.AddHours(-1).AddMinutes(index));
            runs.Add(run.Root);
        }

        Assert.True(Directory.Exists(active.Root));
        Assert.True(File.Exists(notes));
        Assert.False(Directory.Exists(runs[0]));
        Assert.All(runs.Skip(1), path => Assert.True(File.Exists(Path.Combine(path, "logs", "diagnostic.txt"))));
        Assert.False(File.Exists(Path.Combine(root, "result-0.trx")));
        Assert.Equal(5, Directory.GetFiles(root, "*.trx").Length);
    }

    private static void Write(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "retention test");
    }
}

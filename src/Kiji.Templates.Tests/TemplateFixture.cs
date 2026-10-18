using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using CliWrap;
using Xunit;

namespace Kiji.Templates.Tests;

[CollectionDefinition("Templates")]
public sealed class TemplateCollection : ICollectionFixture<TemplateFixture>;

public sealed class TemplateFixture : IAsyncLifetime
{
    internal static string Repository { get; } = FindRepository();
    internal static string PackagePath { get; } = FindPackage();
    internal static Template[] Templates { get; } = ReadTemplates();

    private string RunDirectory { get; } = Path.Combine(
        Repository, "TestResults", $"tests-{Guid.NewGuid():N}");
    internal string Logs { get; }
    private readonly Dictionary<(string, bool), Task<PublishedSite>> sites = [];
    private readonly HashSet<string> projects = [];
    private bool preserveBuildOutput;

    public TemplateFixture() => Logs = Directory.CreateDirectory(Path.Combine(RunDirectory, "logs")).FullName;

    public async ValueTask InitializeAsync() => await RunDotnet("install", "new", "install", PackagePath);

    public ValueTask DisposeAsync()
    {
        if (!preserveBuildOutput && !TestContext.Current.CancellationToken.IsCancellationRequested)
        {
            foreach (var project in projects)
            {
                foreach (var name in new[] { "bin", "obj" })
                {
                    var directory = Path.GetFullPath(Path.Combine(project, name));
                    Assert.StartsWith(Path.GetFullPath(RunDirectory) + Path.DirectorySeparatorChar, directory,
                        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                    if (Directory.Exists(directory))
                        Directory.Delete(directory, recursive: true);
                }
            }
        }
        return ValueTask.CompletedTask;
    }

    // Called from each test's teardown, after assertions and browser cleanup.
    internal void RecordTestResult(bool cleanupSucceeded = true) =>
        preserveBuildOutput |= !cleanupSucceeded || TestContext.Current.TestState?.Result != TestResult.Passed;

    internal Task ValidatePackage() => RunDotnet("validate",
        "msbuild", Path.Combine(Repository, "src", "Kiji.Templates", "Kiji.Templates.csproj"), "-restore",
        "-target:ValidateTemplatesAfterBuild", $"-property:ValidateTemplatesPath={PackagePath}",
        $"-bl:{Path.Combine(Logs, "validate.binlog")}");

    internal Task<PublishedSite> GetSite(string kind, bool subpath)
    {
        // A single xUnit collection serializes access and shares builds across browser tests.
        var key = (kind, subpath);
        if (!sites.TryGetValue(key, out var site))
            sites[key] = site = CreateSite(kind, subpath);
        return site;
    }

    private async Task<PublishedSite> CreateSite(string kind, bool subpath)
    {
        var site = await GenerateSite(kind, subpath);
        await Publish(site, noRestore: !subpath);
        return site;
    }

    internal async Task<PublishedSite> GenerateSite(string kind, bool subpath, string? scenario = null)
    {
        var template = Templates.Single(template => template.Kind == kind);
        var name = subpath ? "Travel.Journal" : "MySite";
        var caseName = scenario ?? (subpath ? "subpath" : "root");
        var project = Path.Combine(RunDirectory, "sites", kind, caseName, "MySite");
        projects.Add(project);
        var prefix = subpath ? $"/Kiji.Templates/{kind}/" : "/";
        // Keep one ordinary publish as a smoke test of the user's default path.
        var optimizeBuild = kind != "minimal" || subpath || scenario is not null;
        var site = new PublishedSite(project, Path.Combine(project, "dist"), prefix, name, $"{kind}-{caseName}", optimizeBuild);
        string[] options = subpath ? ["--name", name, "--no-restore"] : [];
        await RunDotnet($"{site.Label}-new",
            ["new", template.ShortName, "--output", project, .. options]);

        Assert.True(File.Exists(Path.Combine(project, $"{name}.csproj")), $"Missing {name}.csproj");
        var programPath = Path.Combine(project, "Program.cs");
        var program = await File.ReadAllTextAsync(programPath);
        Assert.Contains($"Name = \"{name}\"", program);
        Assert.Equal($"# {name}", File.ReadLines(Path.Combine(project, "README.md")).First());
        Assert.Equal(!subpath, File.Exists(Path.Combine(project, "obj", "project.assets.json")));
        foreach (var file in Directory.EnumerateFiles(project, "*", SearchOption.AllDirectories)
            .Where(file => !Path.GetRelativePath(project, file).Split(Path.DirectorySeparatorChar)
                .Any(part => part is "obj" or "bin")))
        {
            Assert.DoesNotContain(template.SourceName, Path.GetRelativePath(project, file));
            if (Path.GetExtension(file) is ".csproj" or ".cs" or ".razor" or ".md")
                Assert.DoesNotContain(template.SourceName, await File.ReadAllTextAsync(file));
        }

        if (subpath)
        {
            Assert.Contains("https://example.com/", program);
            await File.WriteAllTextAsync(programPath, program.Replace("https://example.com/", $"https://example.com{prefix}"));
        }
        return site;
    }

    internal async Task Publish(PublishedSite site, bool noRestore = false)
    {
        var projectFile = Path.Combine(site.Project, $"{site.Name}.csproj");
        string[] buildOptions = site.OptimizeBuild
            ? ["-p:UseCurrentRuntimeIdentifier=true", "-p:SelfContained=false",
                $"-p:CustomAfterMicrosoftCommonTargets={Path.Combine(Repository, ".github", "scripts", "verification.targets")}"]
            : [];
        // dotnet new's automatic restore has no RID. Prepare the same RID that
        // publish will use before exercising --no-restore, without editing the project.
        if (noRestore && site.OptimizeBuild)
            await RunDotnet($"{site.Label}-restore", ["restore", projectFile, .. buildOptions]);
        string[] options = noRestore ? ["--no-restore"] : [];
        await RunDotnet($"{site.Label}-publish",
            ["publish", projectFile, "-c", "Release", "-o", site.Output,
                $"-bl:{Path.Combine(Logs, site.Label)}.binlog", .. buildOptions, .. options]);
        Assert.False(string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"))));
        if (site.OptimizeBuild)
        {
            var nativeFiles = Directory.GetFiles(Path.Combine(site.Project, "bin"), "libSkiaSharp.*", SearchOption.AllDirectories);
            var nativeLibrary = Assert.Single(nativeFiles);
            Assert.Equal(OperatingSystem.IsWindows() ? "libSkiaSharp.dll"
                : OperatingSystem.IsMacOS() ? "libSkiaSharp.dylib" : "libSkiaSharp.so", Path.GetFileName(nativeLibrary));
        }
    }

    private async Task RunDotnet(string label, params string[] arguments)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        // Isolate template registration while keeping the normal SDK state and caches.
        if (arguments is ["new", ..])
            arguments = [.. arguments, "--debug:custom-hive", Path.Combine(RunDirectory, "templates")];
        var command = Cli.Wrap("dotnet")
            .WithArguments(arguments)
            .WithWorkingDirectory(Repository)
            .WithStandardOutputPipe(PipeTarget.ToFile(Path.Combine(Logs, $"{label}.stdout.log")))
            .WithStandardErrorPipe(PipeTarget.ToFile(Path.Combine(Logs, $"{label}.stderr.log")));
        try
        {
            await command.ExecuteAsync(timeout.Token);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"{command} failed. See {Logs}/{label}.*.log", exception);
        }
    }

    private static string FindPackage()
    {
        var path = Path.GetFullPath(Environment.GetEnvironmentVariable("KIJI_TEMPLATE_PACKAGE")
            ?? Path.Combine("src", "Kiji.Templates", "bin",
                typeof(TemplateFixture).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration), Repository);
        return Directory.Exists(path) ? Directory.GetFiles(path, "*.nupkg").Single() : path;
    }

    private static Template[] ReadTemplates()
    {
        using var archive = ZipFile.OpenRead(PackagePath);
        var templates = archive.Entries
            .Where(entry => entry.FullName.Split('/') is ["content", _, ".template.config", "template.json"])
            .Select(entry =>
            {
                using var stream = entry.Open();
                using var json = JsonDocument.Parse(stream);
                var definition = json.RootElement;
                return new Template(entry.FullName.Split('/')[1],
                    definition.GetProperty("shortName").GetString()!,
                    definition.GetProperty("identity").GetString()!,
                    definition.GetProperty("sourceName").GetString()!);
            })
            .OrderBy(template => template.Kind, StringComparer.Ordinal)
            .ToArray();
        if (templates.Length == 0)
            throw new InvalidOperationException($"No templates found in {PackagePath}.");
        return templates;
    }

    private static string FindRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Kiji.Templates.slnx")))
                return directory.FullName;
        throw new InvalidOperationException("Run the tests from a Kiji.Templates checkout.");
    }

    internal sealed record Template(string Kind, string ShortName, string Identity, string SourceName);
}

internal sealed record PublishedSite(string Project, string Output, string Prefix, string Name, string Label, bool OptimizeBuild);

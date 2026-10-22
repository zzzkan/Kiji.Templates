using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Xunit;

namespace Kiji.Templates.Tests;

[CollectionDefinition("Templates")]
public sealed class TemplateCollection : ICollectionFixture<TemplateFixture>;

public sealed class TemplateFixture : IAsyncLifetime
{
    internal static string Repository { get; } = FindRepository();
    internal static string PackagePath { get; } = FindPackage();
    internal static Template[] Templates { get; } = ReadTemplates();

    private readonly TestRunDirectory run = new(Path.Combine(Repository, "TestResults"));
    private string RunDirectory => run.Root;
    internal string Logs { get; }
    private readonly Dictionary<(string, bool), Task<PublishedSite>> sites = [];

    public TemplateFixture() => Logs = Directory.CreateDirectory(Path.Combine(RunDirectory, "logs")).FullName;

    public async ValueTask InitializeAsync() => await RunDotnet("new", "install", PackagePath);

    public ValueTask DisposeAsync()
    {
        run.Dispose();
        return ValueTask.CompletedTask;
    }

    internal Task ValidatePackage() => RunDotnet(
        "msbuild", Path.Combine(Repository, "src", "Kiji.Templates", "Kiji.Templates.csproj"), "-restore",
        "-target:ValidateTemplatesAfterBuild", $"-property:ValidateTemplatesPath={PackagePath}");

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

    internal async Task<PublishedSite> GenerateSite(string kind, bool subpath, string? scenario = null,
        IReadOnlyDictionary<string, string>? siteInfo = null)
    {
        var template = Templates.Single(template => template.Kind == kind);
        var name = subpath ? "Travel.Journal" : "MySite";
        var caseName = scenario ?? (subpath ? "subpath" : "root");
        var project = run.ProjectDirectory(kind, caseName);
        var prefix = subpath ? $"/Kiji.Templates/{kind}/" : "/";
        // Keep one ordinary publish as a smoke test of the user's default path.
        var optimizeBuild = kind != "minimal" || subpath || scenario is not null;
        var site = new PublishedSite(project, Path.Combine(project, "dist"), prefix, name, optimizeBuild);
        string[] options = subpath
            ? ["--name", name, "--base-url", $"https://example.com{prefix}", "--no-restore"] : [];
        await RunDotnet(
            ["new", template.ShortName, "--output", project, .. options,
                .. siteInfo?.SelectMany(pair => new[] { pair.Key, pair.Value }) ?? []]);

        Assert.True(File.Exists(Path.Combine(project, $"{name}.csproj")), $"Missing {name}.csproj");
        var programPath = Path.Combine(project, "Program.cs");
        var program = await File.ReadAllTextAsync(programPath);
        if (siteInfo is null)
            Assert.Contains($"Name = \"{template.DefaultSiteName}\"", program);
        Assert.Equal($"# {name}", File.ReadLines(Path.Combine(project, "README.md")).First());
        Assert.Equal(!subpath, File.Exists(Path.Combine(project, "obj", "project.assets.json")));
        foreach (var file in Directory.EnumerateFiles(project, "*", SearchOption.AllDirectories)
            .Where(file => !Path.GetRelativePath(project, file).Split(Path.DirectorySeparatorChar)
                .Any(part => part is "obj" or "bin")))
        {
            Assert.DoesNotContain(template.SourceName, Path.GetRelativePath(project, file));
            if (Path.GetExtension(file) is ".csproj" or ".cs" or ".razor" or ".md")
            {
                var content = await File.ReadAllTextAsync(file);
                Assert.DoesNotContain(template.SourceName, content);
                Assert.DoesNotContain("__KIJI_", content);
            }
        }

        Assert.Contains($"BaseUrl = new Uri(\"https://example.com{prefix}\")", program);
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
            await RunDotnet(["restore", projectFile, .. buildOptions]);
        string[] options = noRestore ? ["--no-restore"] : [];
        await RunDotnet(["publish", projectFile, "-c", "Release", "-o", site.Output, .. buildOptions, .. options]);
        Assert.False(string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"))));
        if (site.OptimizeBuild)
        {
            var nativeFiles = Directory.GetFiles(Path.Combine(site.Project, "bin"), "libSkiaSharp.*", SearchOption.AllDirectories);
            var nativeLibrary = Assert.Single(nativeFiles);
            Assert.Equal(OperatingSystem.IsWindows() ? "libSkiaSharp.dll"
                : OperatingSystem.IsMacOS() ? "libSkiaSharp.dylib" : "libSkiaSharp.so", Path.GetFileName(nativeLibrary));
        }
    }

    private async Task RunDotnet(params string[] arguments)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        // Isolate template registration while keeping the normal SDK state and caches.
        if (arguments is ["new", ..])
            arguments = [.. arguments, "--debug:custom-hive", Path.Combine(RunDirectory, "templates")];
        var command = Cli.Wrap("dotnet")
            .WithArguments(arguments)
            .WithWorkingDirectory(Repository)
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .WithValidation(CommandResultValidation.None);
        var result = await command.ExecuteBufferedAsync(timeout.Token);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"{command} failed (exit code {result.ExitCode}).\n{result.StandardOutput}\n{result.StandardError}");
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
                var shortName = definition.GetProperty("shortName").GetString()!;
                return new Template(shortName["kiji-".Length..], shortName,
                    definition.GetProperty("identity").GetString()!,
                    definition.GetProperty("sourceName").GetString()!,
                    definition.GetProperty("symbols").GetProperty("siteName").GetProperty("defaultValue").GetString()!);
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

    internal sealed record Template(string Kind, string ShortName, string Identity, string SourceName, string DefaultSiteName);
}

internal sealed record PublishedSite(string Project, string Output, string Prefix, string Name, bool OptimizeBuild);

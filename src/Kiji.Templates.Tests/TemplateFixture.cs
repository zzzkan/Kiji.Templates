using Kiji.Templates.Infrastructure;
using Xunit;

namespace Kiji.Templates.Tests;

[CollectionDefinition("Templates")]
public sealed class TemplateCollection : ICollectionFixture<TemplateFixture>;

public sealed class TemplateFixture : IAsyncLifetime
{
    internal static string Repository { get; } = FindRepository();
    internal static TemplatePackage Package { get; } = new(
        Environment.GetEnvironmentVariable("KIJI_TEMPLATE_PACKAGE")
        ?? throw new InvalidOperationException("Set KIJI_TEMPLATE_PACKAGE to an existing .nupkg or a directory containing one package."));

    internal string RunDirectory { get; } = Path.Combine(Path.GetFullPath(
        Environment.GetEnvironmentVariable("KIJI_VERIFICATION_DIRECTORY")
        ?? Path.Combine(Repository, "artifacts", "verification")), $"tests-{Guid.NewGuid():N}");
    internal DotnetCli Dotnet { get; }
    private readonly Dictionary<(string, bool), Task<PublishedSite>> sites = [];

    public TemplateFixture() => Dotnet = new(Repository, RunDirectory);

    public async ValueTask InitializeAsync() => await Dotnet.Run("install", TestContext.Current.CancellationToken,
        "new", "install", Package.Path);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

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
        var template = Package.Templates.Single(template => template.Kind == kind);
        var name = subpath ? "Travel.Journal" : "MySite";
        var caseName = scenario ?? (subpath ? "subpath" : "root");
        var project = Path.Combine(RunDirectory, "sites", kind, caseName, "MySite");
        var prefix = subpath ? $"/Kiji.Templates/{kind}/" : "/";
        var site = new PublishedSite(project, Path.Combine(project, "dist"), prefix, name, $"{kind}-{caseName}");
        string[] options = subpath ? ["--name", name, "--no-restore"] : [];
        await Dotnet.Run($"{site.Label}-new", TestContext.Current.CancellationToken,
            ["new", template.ShortName, "--output", project, .. options]);

        Assert.True(File.Exists(Path.Combine(project, $"{name}.csproj")), $"Missing {name}.csproj");
        Assert.Contains($"Name = \"{name}\"", await File.ReadAllTextAsync(Path.Combine(project, "Program.cs")));
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
            var programPath = Path.Combine(project, "Program.cs");
            var program = await File.ReadAllTextAsync(programPath);
            Assert.Contains("https://example.com/", program);
            await File.WriteAllTextAsync(programPath, program.Replace("https://example.com/", $"https://example.com{prefix}"));
        }
        return site;
    }

    internal async Task Publish(PublishedSite site, bool noRestore = false)
    {
        string[] options = noRestore ? ["--no-restore"] : [];
        await Dotnet.Run($"{site.Label}-publish", TestContext.Current.CancellationToken,
            ["publish", Path.Combine(site.Project, $"{site.Name}.csproj"), "-c", "Release", "-o", site.Output,
                "--disable-build-servers", $"-bl:{Path.Combine(Dotnet.Logs, site.Label)}.binlog", .. options]);
        Assert.False(string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"))));
    }

    private static string FindRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Kiji.Templates.slnx")))
                return directory.FullName;
        throw new InvalidOperationException("Run the tests from a Kiji.Templates checkout.");
    }
}

internal sealed record PublishedSite(string Project, string Output, string Prefix, string Name, string Label);

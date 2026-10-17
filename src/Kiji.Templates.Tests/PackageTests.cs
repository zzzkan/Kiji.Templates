using System.IO.Compression;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class PackageTests(TemplateFixture fixture)
{
    [Fact]
    public async Task PackageDefinitionsAreValid() => await fixture.Dotnet.Run("validate", TestContext.Current.CancellationToken,
        "msbuild", Path.Combine(TemplateFixture.Repository, "src", "Kiji.Templates", "Kiji.Templates.csproj"), "-restore",
        "-target:ValidateTemplatesAfterBuild", $"-property:ValidateTemplatesPath={TemplateFixture.Package.Path}",
        $"-bl:{Path.Combine(fixture.Dotnet.Logs, "validate.binlog")}", "-nr:false");

    [Fact]
    public void PackageContainsAllTemplatesWithoutBuildOutput()
    {
        var sourceKinds = Directory.EnumerateDirectories(Path.Combine(TemplateFixture.Repository, "src", "Kiji.Templates", "content"))
            .Where(directory => File.Exists(Path.Combine(directory, ".template.config", "template.json")))
            .Select(Path.GetFileName).Order(StringComparer.Ordinal);
        Assert.Equal(sourceKinds, TemplateFixture.Package.Templates.Select(template => template.Kind));

        using var archive = ZipFile.OpenRead(TemplateFixture.Package.Path);
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith("content/", StringComparison.Ordinal)
            && entry.FullName.Split('/').Any(part => part is "bin" or "obj" or "dist" or ".kiji" or ".vs"
                or ".git" or ".github" or ".agents" or "NuGet.Config" or "nuget.config" or "global.json"));
        foreach (var template in TemplateFixture.Package.Templates)
        {
            Assert.Equal($"kiji-{template.Kind}", template.ShortName);
            Assert.Equal(template.Identity, template.SourceName);
            Assert.StartsWith("Kiji.", template.Identity);
        }
    }
}

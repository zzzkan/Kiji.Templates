using System.IO.Compression;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class PackageTests(TemplateFixture fixture) : IDisposable
{
    public void Dispose() => fixture.RecordTestResult();

    [Fact]
    public Task PackageDefinitionsAreValid() => fixture.ValidatePackage();

    [Fact]
    public void PackageContainsAllTemplatesWithoutBuildOutput()
    {
        var sourceKinds = Directory.EnumerateDirectories(Path.Combine(TemplateFixture.Repository, "src", "Kiji.Templates", "content"))
            .Where(directory => File.Exists(Path.Combine(directory, ".template.config", "template.json")))
            .Select(Path.GetFileName).Order(StringComparer.Ordinal);
        Assert.Equal(sourceKinds, TemplateFixture.Templates.Select(template => template.Kind));

        using var archive = ZipFile.OpenRead(TemplateFixture.PackagePath);
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith("content/", StringComparison.Ordinal)
            && entry.FullName.Split('/').Any(part => part is "bin" or "obj" or "dist" or ".kiji" or ".vs"
                or ".git" or ".github" or ".agents" or "NuGet.Config" or "nuget.config" or "global.json"));
        foreach (var template in TemplateFixture.Templates)
        {
            Assert.Equal($"kiji-{template.Kind}", template.ShortName);
            Assert.Equal(template.Identity, template.SourceName);
            Assert.StartsWith("Kiji.", template.Identity);
        }
    }
}

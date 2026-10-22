using System.IO.Compression;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class PackageTests(TemplateFixture fixture)
{
    [Fact]
    public Task PackageDefinitionsAreValid() => fixture.ValidatePackage();

    [Fact]
    public void PackageContainsAllTemplatesWithoutBuildOutput()
    {
        var sourceKinds = Directory.EnumerateDirectories(Path.Combine(TemplateFixture.Repository, "src", "Kiji.Templates", "content"))
            .Where(directory => File.Exists(Path.Combine(directory, ".template.config", "template.json")))
            .Select(Path.GetFileName).Order(StringComparer.Ordinal);
        Assert.Equal(sourceKinds, TemplateFixture.Templates.Select(template => template.SourceName));

        using var archive = ZipFile.OpenRead(TemplateFixture.PackagePath);
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith("content/", StringComparison.Ordinal)
            && (entry.FullName.Split('/') is ["content", _, "dist", ..]
                || entry.FullName.Split('/').Any(part => part is "bin" or "obj" or ".kiji" or ".vs"
                    or ".git" or ".github" or ".agents" or "NuGet.Config" or "nuget.config" or "global.json")));
        foreach (var template in TemplateFixture.Templates)
        {
            Assert.Equal($"kiji-{template.Kind}", template.ShortName);
            Assert.Equal(template.Identity, template.SourceName);
            Assert.StartsWith("Kiji.", template.Identity);
            Assert.NotNull(archive.GetEntry($"content/{template.SourceName}/{template.SourceName}.csproj"));
        }
    }

    [Fact]
    public void BlogDoesNotBundlePrismAssets()
    {
        using var archive = ZipFile.OpenRead(TemplateFixture.PackagePath);
        const string root = "content/Kiji.Blog/wwwroot/";
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith(root + "lib/prism/", StringComparison.Ordinal));
        Assert.Null(archive.GetEntry(root + "css/prism.css"));
    }

    [Fact]
    public void InteractiveIncludesThirdPartyAssetsAndLicenses()
    {
        using var archive = ZipFile.OpenRead(TemplateFixture.PackagePath);
        const string root = "content/Kiji.Interactive/wwwroot/";
        Assert.NotNull(archive.GetEntry(root + "lib/bootstrap/dist/css/bootstrap.min.css"));
        Assert.NotNull(archive.GetEntry(root + "lib/bootstrap/dist/css/bootstrap.min.css.map"));
        using var bootstrap = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(root + "lib/bootstrap/LICENSE.txt")).Open());
        Assert.Contains("Copyright (c) 2011-2024 The Bootstrap Authors", bootstrap.ReadToEnd());
        using var notices = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(root + "THIRD-PARTY-NOTICES.txt")).Open());
        var text = notices.ReadToEnd();
        Assert.Contains("Copyright (c) .NET Foundation and Contributors", text);
        Assert.Contains("Copyright (c) 2019-2024 The Bootstrap Authors", text);
        Assert.Contains("Permission is hereby granted", text);
    }
}

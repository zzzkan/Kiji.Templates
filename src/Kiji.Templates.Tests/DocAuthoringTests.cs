using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class DocAuthoringTests(TemplateFixture fixture)
{
    [Fact]
    public async Task NewArticlesAppearInNavigationAndSitemapInOrder()
    {
        var site = await fixture.GenerateSite("doc", subpath: true, scenario: "added-articles");
        var articles = new[]
        {
            (File: "z-first/index.md", Slug: "z-first", Order: -10),
            (File: "a-second.md", Slug: "a-second", Order: 5),
            (File: "b-third/index.md", Slug: "b-third", Order: 5),
        };
        foreach (var article in articles)
        {
            var path = Path.Combine(site.Project, "content", article.File);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, $$"""
                ---
                title: {{article.Slug}}
                description: Added article summary.
                order: {{article.Order}}
                ---

                ## Details

                This **new article** is discovered automatically.
                """, TestContext.Current.CancellationToken);
        }
        await fixture.Publish(site);
        var home = await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"), TestContext.Current.CancellationToken);
        var previousPosition = -1;
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = XDocument.Load(Path.Combine(site.Output, "sitemap.xml"))
            .Descendants(ns + "loc").Select(location => location.Value).ToArray();
        foreach (var article in articles)
        {
            var link = $"href=\"{site.Prefix}docs/{article.Slug}/\"";
            var position = home.IndexOf(link, StringComparison.Ordinal);
            Assert.True(position > previousPosition);
            previousPosition = position;
            var html = await File.ReadAllTextAsync(Path.Combine(site.Output, "docs", article.Slug, "index.html"),
                TestContext.Current.CancellationToken);
            Assert.Single(Regex.Matches(html, "<h1[\\s>]"));
            Assert.Contains("<strong>new article</strong>", html);
            Assert.Contains("content=\"Added article summary.\"", html);
            Assert.Contains($"https://example.com{site.Prefix}docs/{article.Slug}/", urls);
        }
    }

    [Theory]
    [InlineData("missing-title")]
    [InlineData("duplicate-slug")]
    public async Task InvalidArticlesFailWithUsefulDiagnostics(string scenario)
    {
        var site = await fixture.GenerateSite("doc", subpath: true, scenario: scenario);
        var path = Path.Combine(site.Project, "content", "getting-started.md");
        if (scenario == "duplicate-slug")
            File.Copy(Path.Combine(site.Project, "content", "getting-started", "index.md"), path);
        else
            await File.WriteAllTextAsync(path, "---\ndescription: Missing a title.\n---\n\nContent.",
                TestContext.Current.CancellationToken);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Publish(site));
        Assert.Contains(scenario == "duplicate-slug" ? "duplicate output path" : "articles require a title", exception.Message);
    }
}

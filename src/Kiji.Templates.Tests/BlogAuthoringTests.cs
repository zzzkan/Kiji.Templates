using System.Xml.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class BlogAuthoringTests(TemplateFixture fixture) : IDisposable
{
    public void Dispose() => fixture.RecordTestResult();

    [Fact]
    public async Task MarkdownPostsAndGeneralPagesPublishInTheirOwnCollections()
    {
        var site = await fixture.GenerateSite("blog", subpath: true, scenario: "added-posts");
        var posts = new[]
        {
            (File: "bundled-post/index.md", Slug: "bundled-post"),
            (File: "single-post.md", Slug: "single-post"),
        };
        foreach (var post in posts)
        {
            var path = Path.Combine(site.Project, "content", "posts", post.File);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, $$"""
                ---
                title: {{post.Slug}}
                description: Added during template verification.
                date: 2026-03-01T09:00:00Z
                ---

                This **new post** must be published automatically.
                """, TestContext.Current.CancellationToken);
        }
        var pages = new[]
        {
            (File: "reading/index.md", Slug: "reading"),
            (File: "contact.md", Slug: "contact"),
        };
        foreach (var page in pages)
        {
            var path = Path.Combine(site.Project, "content", "pages", page.File);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, $$"""
                ---
                title: {{page.Slug}}
                description: A general page without a date.
                ---

                ## Details

                This **general page** must be published automatically.
                """, TestContext.Current.CancellationToken);
        }
        Assert.False(File.Exists(Path.Combine(site.Project, "content", "pages", "home.md")));
        // Markdown outside either collection is ignored.
        await File.WriteAllTextAsync(Path.Combine(site.Project, "content", "about.md"),
            "# Not a blog post", TestContext.Current.CancellationToken);
        await fixture.Publish(site);

        var home = await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"), TestContext.Current.CancellationToken);
        Assert.Contains("<title>Home - Kiji Blog</title>", home);
        Assert.Contains("A place for small notes</h1>", home);
        var items = XDocument.Load(Path.Combine(site.Output, "feed.xml")).Root!.Element("channel")!.Elements("item").ToArray();
        Assert.Equal(4, items.Length);
        Assert.Equal(posts.Select(post => post.Slug), items.Take(2).Select(item => item.Element("title")!.Value));
        XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemapUrls = XDocument.Load(Path.Combine(site.Output, "sitemap.xml"))
            .Descendants(sitemapNamespace + "loc").Select(location => location.Value).ToArray();
        Assert.Equal(9, sitemapUrls.Length);
        foreach (var post in posts)
        {
            Assert.Contains($"href=\"{site.Prefix}posts/{post.Slug}/\"", home);
            Assert.Contains("<strong>new post</strong>",
                await File.ReadAllTextAsync(Path.Combine(site.Output, "posts", post.Slug, "index.html"), TestContext.Current.CancellationToken));
            Assert.Contains(items, item => item.Element("link")!.Value == $"https://example.com{site.Prefix}posts/{post.Slug}/");
            Assert.Contains($"https://example.com{site.Prefix}posts/{post.Slug}/", sitemapUrls);
        }
        foreach (var page in pages)
        {
            var html = await File.ReadAllTextAsync(Path.Combine(site.Output, page.Slug, "index.html"), TestContext.Current.CancellationToken);
            Assert.Single(Regex.Matches(html, "<h1[\\s>]"));
            Assert.Contains($"<h1>{page.Slug}</h1>", html);
            Assert.Contains("Details</h2>", html);
            Assert.Contains("<strong>general page</strong>", html);
            Assert.Contains($"https://example.com{site.Prefix}{page.Slug}/", html);
            Assert.DoesNotContain($"href=\"{site.Prefix}{page.Slug}/\"", home);
            Assert.DoesNotContain(items, item => item.Element("title")!.Value == page.Slug);
            Assert.Contains($"https://example.com{site.Prefix}{page.Slug}/", sitemapUrls);
        }
    }

    [Theory]
    [InlineData("posts", "first-note", "posts/first-note/index.html")]
    [InlineData("pages", "about", "about/index.html")]
    public async Task ContentWithTheSameSlugCannotBePublished(string collection, string slug, string output)
    {
        var site = await fixture.GenerateSite("blog", subpath: false, scenario: $"duplicate-{collection}-slug");
        var content = Path.Combine(site.Project, "content", collection);
        var bundle = Path.Combine(content, slug, "index.md");
        var single = Path.Combine(content, $"{slug}.md");
        Directory.CreateDirectory(Path.GetDirectoryName(bundle)!);
        if (File.Exists(bundle))
            File.Copy(bundle, single);
        else
            File.Copy(single, bundle);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Publish(site));
        Assert.Contains($"duplicate output path '{output.Replace('/', Path.DirectorySeparatorChar)}'", exception.Message);
    }
}

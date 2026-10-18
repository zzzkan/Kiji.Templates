using System.Xml.Linq;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class BlogAuthoringTests(TemplateFixture fixture) : IDisposable
{
    public void Dispose() => fixture.RecordTestResult();

    [Fact]
    public async Task AddedMarkdownPostsAppearInPagesHomeAndFeed()
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
        // Markdown outside the post collection must not require post metadata or be published.
        await File.WriteAllTextAsync(Path.Combine(site.Project, "content", "about.md"),
            "# Not a blog post", TestContext.Current.CancellationToken);
        await fixture.Publish(site);

        var home = await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"), TestContext.Current.CancellationToken);
        var items = XDocument.Load(Path.Combine(site.Output, "feed.xml")).Root!.Element("channel")!.Elements("item").ToArray();
        Assert.Equal(4, items.Length);
        Assert.Equal(posts.Select(post => post.Slug), items.Take(2).Select(item => item.Element("title")!.Value));
        foreach (var post in posts)
        {
            Assert.Contains($"href=\"{site.Prefix}posts/{post.Slug}/\"", home);
            Assert.Contains("<strong>new post</strong>",
                await File.ReadAllTextAsync(Path.Combine(site.Output, "posts", post.Slug, "index.html"), TestContext.Current.CancellationToken));
            Assert.Contains(items, item => item.Element("link")!.Value == $"https://example.com{site.Prefix}posts/{post.Slug}/");
        }
    }

    [Fact]
    public async Task PostsWithTheSameSlugCannotBePublished()
    {
        var site = await fixture.GenerateSite("blog", subpath: false, scenario: "duplicate-slug");
        var posts = Path.Combine(site.Project, "content", "posts");
        File.Copy(Path.Combine(posts, "first-note", "index.md"), Path.Combine(posts, "first-note.md"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Publish(site));

        var stdout = await File.ReadAllTextAsync(Path.Combine(fixture.Logs, $"{site.Label}-publish.stdout.log"), TestContext.Current.CancellationToken);
        var stderr = await File.ReadAllTextAsync(Path.Combine(fixture.Logs, $"{site.Label}-publish.stderr.log"), TestContext.Current.CancellationToken);
        Assert.Contains($"duplicate output path '{Path.Combine("posts", "first-note", "index.html")}'", stdout + stderr);
    }
}

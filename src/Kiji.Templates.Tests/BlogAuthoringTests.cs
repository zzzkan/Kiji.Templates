using System.Xml.Linq;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class BlogAuthoringTests(TemplateFixture fixture) : IDisposable
{
    public void Dispose() => fixture.RecordTestResult();

    [Fact]
    public async Task AddedMarkdownPostAppearsInPageHomeAndFeed()
    {
        var site = await fixture.GenerateSite("blog", subpath: true, scenario: "added-post");
        var post = Directory.CreateDirectory(Path.Combine(site.Project, "contents", "verification-post"));
        await File.WriteAllTextAsync(Path.Combine(post.FullName, "index.md"), """
            ---
            title: A newly added post
            description: Added during template verification.
            date: 2026-03-01T09:00:00Z
            ---

            This **new post** must be published automatically.
            """, TestContext.Current.CancellationToken);
        await fixture.Publish(site);

        Assert.Contains($"href=\"{site.Prefix}posts/verification-post/\"",
            await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"), TestContext.Current.CancellationToken));
        Assert.Contains("<strong>new post</strong>",
            await File.ReadAllTextAsync(Path.Combine(site.Output, "posts", "verification-post", "index.html"), TestContext.Current.CancellationToken));
        var items = XDocument.Load(Path.Combine(site.Output, "feed.xml")).Root!.Element("channel")!.Elements("item").ToArray();
        Assert.Equal(3, items.Length);
        Assert.Equal("A newly added post", items[0].Element("title")!.Value);
        Assert.Equal($"https://example.com{site.Prefix}posts/verification-post/", items[0].Element("link")!.Value);
    }
}

using System.Xml.Linq;
using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.Tests;

public class BlogTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        JavaScriptEnabled = false,
        ColorScheme = ColorScheme.Light,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    [Theory]
    [InlineData(false, "/", "Kiji Blog")]
    [InlineData(true, "/Kiji.Templates/blog/", "Kiji Blog")]
    public Task PublishedBlogWorksWithoutJavaScript(bool subpath, string prefix, string siteName)
        => VerifySite("blog", subpath, async (baseUrl, diagnostics) =>
    {
        await Page.GotoAsync(baseUrl);
        await Expect(Page).ToHaveTitleAsync($"Home - {siteName}");
        await Expect(Page.GetByRole(AriaRole.Main)).ToMatchAriaSnapshotAsync($"""
            - main:
              - /children: deep-equal
              - heading "A place for small notes" [level=1]
              - paragraph: A Markdown blog with RSS and light/dark colors.
              - region "Posts":
                - heading "Posts" [level=2]
                - list:
                  - listitem:
                    - time: 03 Feb 2026
                    - link "Keeping a notebook":
                      - /url: {prefix}posts/keeping-a-notebook/
                  - listitem:
                    - time: 12 Jan 2026
                    - link "A first note":
                      - /url: {prefix}posts/first-note/
            """);
        await File.WriteAllTextAsync(Path.Combine(diagnostics, "home.aria.yml"),
            await Page.GetByRole(AriaRole.Main).AriaSnapshotAsync(), TestContext.Current.CancellationToken);
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href", $"https://example.com{prefix}");
        await Expect(Page.Locator("meta[property='og:title']")).ToHaveAttributeAsync("content", $"Home - {siteName}");
        await Expect(Page.Locator("meta[property='og:type']")).ToHaveAttributeAsync("content", "website");
        await Expect(Page.Locator("meta[property='og:url']")).ToHaveAttributeAsync("content", $"https://example.com{prefix}");
        await Expect(Page.Locator("meta[name=author]")).ToHaveAttributeAsync("content", "zzzkan");
        var favicon = await Page.Locator("link[rel=icon]").GetAttributeAsync("href");
        Assert.StartsWith("data:image/svg+xml,", favicon);
        Assert.Contains("📓", Uri.UnescapeDataString(favicon!));
        await Expect(Page.GetByRole(AriaRole.Contentinfo)).ToContainTextAsync($"© {DateTime.UtcNow.Year} zzzkan");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Home", Exact = true })).ToHaveAttributeAsync("aria-current", "page");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Skip to content" })).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.GetByRole(AriaRole.Main)).ToBeFocusedAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "A first note", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("A first note");
        await Expect(Page.Locator("time")).ToHaveAttributeAsync("datetime", "2026-01-12");
        await Expect(Page.Locator("meta[name=description]")).ToHaveAttributeAsync("content", "Making a little room for writing.");
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href", $"https://example.com{prefix}posts/first-note/");
        await Expect(Page.Locator("meta[property='og:type']")).ToHaveAttributeAsync("content", "article");
        await Expect(Page.Locator("meta[property='og:description']")).ToHaveAttributeAsync("content", "Making a little room for writing.");
        Assert.Equal(DateTimeOffset.Parse("2026-01-12T09:00:00Z"), DateTimeOffset.Parse(
            (await Page.Locator("meta[property='article:published_time']").GetAttributeAsync("content"))!));
        // Markdown's document-relative link must preserve the deployment prefix.
        await Page.GetByRole(AriaRole.Link, new() { Name = "keeping a notebook", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{baseUrl}posts/keeping-a-notebook/");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Keeping a notebook");
        await Expect(Page.Locator("pre code.language-text")).ToContainTextAsync("Notice something.");
        await Expect(Page.Locator("pre code.language-csharp")).ToContainTextAsync("Console.WriteLine(note);");
        var picture = Page.GetByRole(AriaRole.Img, new() { Name = "Three steps for a notebook habit: notice, write, and return." });
        await Expect(picture).ToBeVisibleAsync();
        await Expect(picture).ToHaveAttributeAsync("srcset", new System.Text.RegularExpressions.Regex(@"\d+w"));
        Assert.True(await picture.EvaluateAsync<bool>("image => image.complete && image.naturalWidth > 0"));
        Assert.StartsWith($"{baseUrl}posts/keeping-a-notebook/", await picture.EvaluateAsync<string>("image => image.currentSrc"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "post-desktop.png"), FullPage = true });

        await Page.SetViewportSizeAsync(390, 844);
        await Expect(Page.GetByRole(AriaRole.Navigation)).ToBeVisibleAsync();
        await AssertNoOverflow();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "post-mobile.png"), FullPage = true });
        var lightBackground = await Page.Locator("html").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        var darkBackground = await Page.Locator("html").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.NotEqual(lightBackground, darkBackground);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "post-dark.png"), FullPage = true });
        await Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Home", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile.png"), FullPage = true });
        await Page.SetViewportSizeAsync(320, 640);
        await AssertNoOverflow();

        await Page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("About");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true })).ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("I write to make sense of what I am learning");
        await Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Policy", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{baseUrl}policy/");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Policy");
        await Expect(Page.Locator(".prose h1")).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 2, Name = "Privacy", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href", $"https://example.com{prefix}policy/");
        await Expect(Page.Locator("meta[property='og:type']")).ToHaveAttributeAsync("content", "website");
        await Expect(Page.Locator("meta[property='article:published_time']")).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Policy" })).ToHaveAttributeAsync("aria-current", "page");
        await AssertNoOverflow();
        await Expect(Page.Locator("a[href$='feed.xml'], a[href$='sitemap.xml']")).ToHaveCountAsync(0);
        await Expect(Page.Locator("link[rel=alternate]")).ToHaveAttributeAsync("href", $"{prefix}feed.xml");
        using var http = new HttpClient();
        var feed = XDocument.Parse(await http.GetStringAsync($"{baseUrl}feed.xml", TestContext.Current.CancellationToken));
        var channel = feed.Root!.Element("channel")!;
        Assert.Equal(siteName, channel.Element("title")!.Value);
        Assert.Equal(["Keeping a notebook", "A first note"], channel.Elements("item").Select(item => item.Element("title")!.Value));
        Assert.Equal($"https://example.com{prefix}posts/keeping-a-notebook/", channel.Elements("item").First().Element("link")!.Value);

        var sitemap = XDocument.Parse(await http.GetStringAsync($"{baseUrl}sitemap.xml", TestContext.Current.CancellationToken));
        XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        Assert.Equal(sitemapNamespace + "urlset", sitemap.Root!.Name);
        Assert.Equal(new[] { "", "about/", "policy/", "posts/first-note/", "posts/keeping-a-notebook/" }
            .Select(path => $"https://example.com{prefix}{path}"),
            sitemap.Root.Elements(sitemapNamespace + "url").Select(url => url.Element(sitemapNamespace + "loc")!.Value));

        await Page.GotoAsync($"{baseUrl}404.html");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Page not found");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Return to the posts", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
    });

    private async Task AssertNoOverflow() => Assert.True(await Page.EvaluateAsync<bool>(
        "document.documentElement.scrollWidth <= window.innerWidth"));
}

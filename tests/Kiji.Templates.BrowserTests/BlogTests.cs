using System.Xml.Linq;
using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.BrowserTests;

public class BlogTests : PublishedSiteTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        JavaScriptEnabled = false,
        ColorScheme = ColorScheme.Light,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    [Theory]
    [InlineData("MySite/dist", "/", "MySite", "root")]
    [InlineData("subpath", "/Kiji.Templates/blog/", "Travel.Journal", "subpath")]
    public Task PublishedBlogWorksWithoutJavaScript(string siteDirectory, string prefix, string siteName, string caseName)
        => VerifySite("blog", siteDirectory, prefix, caseName, async (baseUrl, diagnostics) =>
    {
        await Page.GotoAsync(baseUrl);
        await Expect(Page).ToHaveTitleAsync($"Home - {siteName}");
        await Expect(Page.GetByRole(AriaRole.Main)).ToMatchAriaSnapshotAsync($"""
            - main:
              - /children: deep-equal
              - heading "A place for small notes" [level=1]
              - paragraph: Notes on things made, read, and noticed.
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
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Home", Exact = true })).ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.Locator("script")).ToHaveCountAsync(0);
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
        // Markdown's document-relative link must preserve the deployment prefix.
        await Page.GetByRole(AriaRole.Link, new() { Name = "keeping a notebook", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{baseUrl}posts/keeping-a-notebook/");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Keeping a notebook");
        await Expect(Page.Locator("pre code")).ToContainTextAsync("Notice something.");
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
        await Page.GetByRole(AriaRole.Link, new() { Name = "All posts", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile.png"), FullPage = true });
        await Page.SetViewportSizeAsync(320, 640);
        await AssertNoOverflow();

        await Page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("About");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true })).ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "RSS", Exact = true })).ToHaveAttributeAsync("href", $"{prefix}feed.xml");
        await Expect(Page.Locator("link[rel=alternate]")).ToHaveAttributeAsync("href", $"{prefix}feed.xml");
        using var http = new HttpClient();
        var feed = XDocument.Parse(await http.GetStringAsync($"{baseUrl}feed.xml", TestContext.Current.CancellationToken));
        var channel = feed.Root!.Element("channel")!;
        Assert.Equal(siteName, channel.Element("title")!.Value);
        Assert.Equal(["Keeping a notebook", "A first note"], channel.Elements("item").Select(item => item.Element("title")!.Value));
        Assert.Equal($"https://example.com{prefix}posts/keeping-a-notebook/", channel.Elements("item").First().Element("link")!.Value);

        await Page.GotoAsync($"{baseUrl}404.html");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Page not found");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Return to the posts", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
    });

    private async Task AssertNoOverflow() => Assert.True(await Page.EvaluateAsync<bool>(
        "document.documentElement.scrollWidth <= window.innerWidth"));
}

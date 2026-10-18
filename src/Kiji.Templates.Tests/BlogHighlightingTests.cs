using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.Tests;

public sealed class BlogHighlightingTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ColorScheme = ColorScheme.Light,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task CodeIsHighlightedWithCdnPrismInBothColorSchemes(bool subpath)
        => VerifySite("blog", subpath, async (baseUrl, diagnostics) =>
    {
        var requests = new System.Collections.Concurrent.ConcurrentQueue<string>();
        Page.Request += (_, request) => requests.Enqueue(request.Url);
        await Page.GotoAsync($"{baseUrl}posts/keeping-a-notebook/");
        var code = Page.Locator("pre code.language-csharp");
        var keyword = code.Locator(".token.keyword").First;
        await Expect(keyword).ToHaveTextAsync("var");
        await Expect(code.Locator(".token.string")).ToHaveCountAsync(3);
        Assert.Contains("https://cdn.jsdelivr.net/npm/prismjs@1.30.0/components/prism-csharp.min.js", requests);
        Assert.Contains("https://cdn.jsdelivr.net/npm/prismjs@1.30.0/themes/prism.min.css", requests);
        await Expect(Page.Locator("pre code.language-text .token")).ToHaveCountAsync(0);
        var textColor = await code.EvaluateAsync<string>("element => getComputedStyle(element).color");
        var lightColor = await keyword.EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual(textColor, lightColor);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "highlight-desktop.png"), FullPage = true });

        await Page.SetViewportSizeAsync(390, 844);
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "highlight-mobile.png"), FullPage = true });
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        var darkColor = await keyword.EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual(lightColor, darkColor);
        Assert.NotEqual(await code.EvaluateAsync<string>("element => getComputedStyle(element).color"), darkColor);
        Assert.Contains("https://cdn.jsdelivr.net/npm/prismjs@1.30.0/themes/prism-tomorrow.min.css", requests);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "highlight-dark.png"), FullPage = true });
    });
}

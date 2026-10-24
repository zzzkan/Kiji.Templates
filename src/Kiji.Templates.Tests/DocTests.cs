using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.Tests;

public sealed class DocTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ViewportSize = new() { Width = 1440, Height = 1000 },
        ColorScheme = ColorScheme.Light,
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task DocumentationNavigationAndCodeWork(bool subpath) => VerifySite("doc", subpath, async (baseUrl, diagnostics) =>
    {
        var requests = new List<string>();
        Page.Request += (_, request) => requests.Add(request.Url);
        await Page.GotoAsync(baseUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Kiji Doc");
        var navigation = Page.Locator(".side-nav");
        await Expect(navigation.GetByRole(AriaRole.Link)).ToHaveTextAsync(
            ["Getting started", "Writing documentation", "Deployment"]);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });
        await navigation.GetByRole(AriaRole.Link, new() { Name = "Getting started" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "docs/getting-started/");
        await Expect(navigation.Locator("[aria-current=page]")).ToHaveTextAsync("Getting started");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Getting started");
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href",
            "https://example.com" + new Uri(baseUrl).AbsolutePath + "docs/getting-started/");
        var code = Page.Locator("pre code.language-csharp");
        await Expect(code.Locator(".token.keyword").First).ToHaveTextAsync("new");
        var keyword = code.Locator(".token.keyword").First;
        var lightColor = await keyword.EvaluateAsync<string>("element => getComputedStyle(element).color");
        var lightBackground = await Page.Locator("body").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "article-desktop.png"), FullPage = true });
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(keyword).Not.ToHaveCSSAsync("color", lightColor);
        await Expect(Page.Locator("body")).Not.ToHaveCSSAsync("background-color", lightBackground);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "article-dark.png"), FullPage = true });
        await Page.GetByRole(AriaRole.Main).GetByRole(AriaRole.Link, new() { Name = "write documentation" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "docs/writing/");
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Writing documentation");
        await Page.GotoAsync(baseUrl + "404.html");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Back to home" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
        Assert.DoesNotContain(requests, url => url.Contains("/_framework/", StringComparison.Ordinal)
            || url.EndsWith(".wasm", StringComparison.Ordinal));

        await using var noJs = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            ViewportSize = new() { Width = 390, Height = 844 },
        });
        var mobile = await noJs.NewPageAsync();
        await mobile.GotoAsync(baseUrl);
        await Expect(mobile.Locator(".side-nav")).ToBeHiddenAsync();
        await Expect(mobile.Locator(".mobile-nav")).ToBeHiddenAsync();
        var menu = mobile.GetByRole(AriaRole.Checkbox, new() { Name = "Toggle documentation menu" });
        await menu.FocusAsync();
        await mobile.Keyboard.PressAsync("Space");
        await Expect(menu).ToBeCheckedAsync();
        await Expect(mobile.Locator(".mobile-nav")).ToBeVisibleAsync();
        await mobile.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile-menu.png"), FullPage = true });
        await mobile.GetByRole(AriaRole.Navigation, new() { Name = "Documentation", Exact = true })
            .GetByRole(AriaRole.Link, new() { Name = "Getting started" }).ClickAsync();
        await Expect(mobile).ToHaveURLAsync(baseUrl + "docs/getting-started/");
        await Expect(mobile.Locator("h1")).ToHaveTextAsync("Getting started");
        Assert.True(await mobile.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await mobile.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "article-mobile.png"), FullPage = true });
        await mobile.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await mobile.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "article-mobile-dark.png"), FullPage = true });
    });
}

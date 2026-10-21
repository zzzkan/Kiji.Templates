using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.Tests;

public sealed class FluentTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ViewportSize = new() { Width = 1440, Height = 1000 },
        ColorScheme = ColorScheme.Light,
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task PublishedFluentWorks(bool subpath) => VerifySite("fluent", subpath, async (baseUrl, diagnostics) =>
    {
        var requests = new List<string>();
        Page.Request += (_, request) => requests.Add(request.Url);
        await Page.GotoAsync(baseUrl);
        await Expect(Page).ToHaveTitleAsync("Kiji Fluent");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToContainTextAsync("Your next idea.");
        var cards = Page.Locator("[data-resource]:visible");
        await Expect(cards).ToHaveCountAsync(6);
        var search = Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search resources" });
        await Expect(search).ToBeVisibleAsync();
        // Check the RCL's web-component registration and its scoped card stylesheet.
        Assert.True(await Page.Locator("fluent-button").First.EvaluateAsync<bool>("element => !!element.shadowRoot"));
        await Expect(Page.Locator(".resource-card").First).ToHaveCSSAsync("padding", "26px");
        Assert.Contains(requests, url => url.Contains("/_content/Microsoft.FluentUI.AspNetCore.Components/", StringComparison.Ordinal));
        Assert.Contains(requests, url => url.EndsWith("Microsoft.FluentUI.AspNetCore.Components.bundle.scp.css", StringComparison.Ordinal));
        Assert.DoesNotContain(requests, url => url.Contains("/_framework/", StringComparison.Ordinal)
            || url.EndsWith(".wasm", StringComparison.Ordinal));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });

        var design = Page.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true });
        await design.ClickAsync();
        await Expect(design).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("  FLUENT  ");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("detail");
        await Expect(cards).ToHaveCountAsync(1);
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("1 resource to explore");
        await search.FillAsync("no-such-resource");
        await Expect(cards).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "No matches, yet." })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear filters" }).ClickAsync();
        await Expect(cards).ToHaveCountAsync(6);
        await Expect(search).ToHaveValueAsync("");

        var build = Page.GetByRole(AriaRole.Button, new() { Name = "Build", Exact = true });
        await build.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(cards).ToHaveCountAsync(2);
        await Page.GetByRole(AriaRole.Button, new() { Name = "All", Exact = true }).ClickAsync();
        var start = Page.GetByRole(AriaRole.Button, new() { Name = "Start building" });
        await start.ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Your site starts here." });
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog).ToContainTextAsync("dotnet new kiji-fluent");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "quick-start.png") });
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await start.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Got it" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "The recipe", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "guide/");
        await Expect(Page.Locator("script")).ToHaveCountAsync(0);
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href",
            $"https://example.com{(subpath ? "/Kiji.Templates/fluent/" : "/")}guide/");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "The recipe", Exact = true }))
            .ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "What runs where?" })).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "guide-desktop.png"), FullPage = true });

        await Page.SetViewportSizeAsync(390, 844);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Library", Exact = true }).ClickAsync();
        await Expect(search).ToBeVisibleAsync();
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile.png"), FullPage = true });
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(Page.Locator("body")).ToHaveCSSAsync("background-color", "rgb(20, 34, 31)");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile-dark.png"), FullPage = true });
        await start.ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "quick-start-mobile-dark.png") });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Got it" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "The recipe", Exact = true }).ClickAsync();
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "guide-mobile-dark.png"), FullPage = true });

        await Page.GotoAsync(baseUrl + "404.html");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Back to the library" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);

        await using var noJs = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            ViewportSize = new() { Width = 390, Height = 844 },
        });
        var staticPage = await noJs.NewPageAsync();
        await staticPage.GotoAsync(baseUrl);
        await Expect(staticPage.Locator("[data-resource]:visible")).ToHaveCountAsync(6);
        await Expect(staticPage.Locator(".library-tools")).ToBeHiddenAsync();
        await staticPage.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-no-javascript.png"), FullPage = true });
        await staticPage.GetByRole(AriaRole.Link, new() { Name = "The recipe", Exact = true }).ClickAsync();
        await Expect(staticPage).ToHaveURLAsync(baseUrl + "guide/");
    });
}

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
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToContainTextAsync("A stronger digital presence.");
        var cards = Page.Locator("[data-resource]:visible");
        await Expect(cards).ToHaveCountAsync(6);
        var search = Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search resources" });
        await Expect(search).ToBeVisibleAsync();
        await Expect(Page.Locator(".brand .emoji").First).ToHaveTextAsync("🏢");
        await Expect(Page.Locator(".resource-card .emoji")).ToHaveCountAsync(6);
        await Expect(Page.Locator(".emoji svg, .emoji img")).ToHaveCountAsync(0);
        // Check the RCL's web-component registration and its scoped card stylesheet.
        Assert.True(await Page.Locator("fluent-button").First.EvaluateAsync<bool>("element => !!element.shadowRoot"));
        await Expect(Page.Locator(".resource-card").First).ToHaveCSSAsync("padding", "26px");
        Assert.Contains(requests, url => url.Contains("/_content/Microsoft.FluentUI.AspNetCore.Components/", StringComparison.Ordinal));
        Assert.Contains(requests, url => url.EndsWith("Microsoft.FluentUI.AspNetCore.Components.bundle.scp.css", StringComparison.Ordinal));
        Assert.DoesNotContain(requests, url => url.Contains("/_framework/", StringComparison.Ordinal)
            || url.EndsWith(".wasm", StringComparison.Ordinal));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-hero.png") });

        var design = Page.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true });
        await design.ClickAsync();
        await Expect(design).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("  FLUENT  ");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("inclusive");
        await Expect(cards).ToHaveCountAsync(1);
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("1 resource to explore");
        await search.FillAsync("no-such-resource");
        await Expect(cards).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "No matching resources." })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear filters" }).ClickAsync();
        await Expect(cards).ToHaveCountAsync(6);
        await Expect(search).ToHaveValueAsync("");

        var build = Page.GetByRole(AriaRole.Button, new() { Name = "Build", Exact = true });
        await build.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(cards).ToHaveCountAsync(2);
        await Page.GetByRole(AriaRole.Button, new() { Name = "All", Exact = true }).ClickAsync();
        var start = Page.GetByRole(AriaRole.Button, new() { Name = "Plan your project" });
        await start.ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Define your next chapter." });
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog).ToContainTextAsync("Your objective");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "project-checklist.png") });
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await start.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Close checklist" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Technology", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "guide/");
        await Expect(Page.Locator("script")).ToHaveCountAsync(0);
        await Expect(Page.Locator("link[rel=canonical]")).ToHaveAttributeAsync("href",
            $"https://example.com{(subpath ? "/Kiji.Templates/fluent/" : "/")}guide/");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Technology", Exact = true }))
            .ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "What runs where?" })).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "guide-desktop.png"), FullPage = true });
        await Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Capabilities", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "#services");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Corporate websites", Exact = true })).ToBeInViewportAsync();
        await Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Resources", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "#resources");
        await Expect(search).ToBeInViewportAsync();

        await Page.SetViewportSizeAsync(390, 844);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Overview", Exact = true }).ClickAsync();
        await Expect(search).ToBeVisibleAsync();
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile.png"), FullPage = true });
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(Page.Locator("body")).ToHaveCSSAsync("background-color", "rgb(13, 23, 41)");
        // Wait for the RCL's theme listener and color transition before capturing.
        await Expect(build).ToHaveCSSAsync("color", "rgb(255, 255, 255)");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile-dark.png"), FullPage = true });
        await start.ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "project-checklist-mobile-dark.png") });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Close checklist" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Technology", Exact = true }).ClickAsync();
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "guide-mobile-dark.png"), FullPage = true });

        await Page.GotoAsync(baseUrl + "404.html");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Back to overview" }).ClickAsync();
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
        await staticPage.GetByRole(AriaRole.Link, new() { Name = "Technology", Exact = true }).ClickAsync();
        await Expect(staticPage).ToHaveURLAsync(baseUrl + "guide/");
    });
}

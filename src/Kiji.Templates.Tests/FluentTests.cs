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
    [InlineData(ColorScheme.Light)]
    [InlineData(ColorScheme.Dark)]
    public Task FluentInitialRenderWaitsForStyles(ColorScheme colorScheme) => VerifySite("fluent", true, async (baseUrl, diagnostics) =>
    {
        await Page.EmulateMediaAsync(new() { ColorScheme = colorScheme });
        var requested = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await Page.RouteAsync("**/Microsoft.FluentUI.AspNetCore.Components.lib.module.js", async route =>
        {
            requested.TrySetResult();
            await release.Task;
            await route.ContinueAsync();
        });
        var navigation = Page.GotoAsync(baseUrl);
        try
        {
            await requested.Task.WaitAsync(TimeSpan.FromSeconds(10));
            // Keep the library pending long enough for an unstyled frame to paint.
            await Task.Delay(300);
            Assert.Empty(await Page.EvaluateAsync<string[]>("performance.getEntriesByType('paint').map(entry => entry.name)"));
        }
        finally
        {
            release.TrySetResult();
            await navigation;
        }

        await Expect(Page.Locator("body")).ToHaveCSSAsync("font-size", "14px");
        Assert.True(await Page.EvaluateAsync<bool>("!!customElements.get('fluent-button')"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, $"initial-{colorScheme}.png"), FullPage = true });
    });

    [Fact]
    public Task PublishedFluentWorks() => VerifySite("fluent", true, async (baseUrl, diagnostics) =>
    {
        var requests = new List<string>();
        Page.Request += (_, request) => requests.Add(request.Url);
        await Page.GotoAsync(baseUrl);
        var cards = Page.Locator("[data-resource]:visible");
        await Expect(cards).ToHaveCountAsync(6);
        var search = Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search resources" });
        await Expect(search).ToBeVisibleAsync();
        var lightBackground = await Page.Locator("body").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.Contains(requests, url => url.EndsWith("Microsoft.FluentUI.AspNetCore.Components.bundle.scp.css", StringComparison.Ordinal));
        Assert.DoesNotContain(requests, url => url.Contains("/_framework/", StringComparison.Ordinal)
            || url.EndsWith(".wasm", StringComparison.Ordinal));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });

        var design = Page.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true });
        await design.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(design).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("  FLUENT  ");
        await Expect(cards).ToHaveCountAsync(2);
        await search.FillAsync("inclusive");
        await Expect(cards).ToHaveCountAsync(1);
        await Expect(cards).ToContainTextAsync("Design for every audience");
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("1 resource to explore");
        // This term matches other categories, so it also checks that filters combine.
        await search.FillAsync("foundation");
        await Expect(cards).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "No matching resources." })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear filters" }).ClickAsync();
        await Expect(cards).ToHaveCountAsync(6);
        await Expect(search).ToHaveValueAsync("");

        await Page.GotoAsync(baseUrl + "404.html");
        await Expect(Page.Locator("body")).ToHaveCSSAsync("background-color", lightBackground);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Back to overview" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl);
        await Page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Resources", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(baseUrl + "#resources");
        await Expect(search).ToBeInViewportAsync();

        await Page.SetViewportSizeAsync(390, 844);
        await Page.GotoAsync(baseUrl);
        await Expect(search).ToBeVisibleAsync();
        Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile.png"), FullPage = true });
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Page.ReloadAsync();
        await Expect(search).ToBeVisibleAsync();
        await Expect(Page.Locator("body")).Not.ToHaveCSSAsync("background-color", lightBackground);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-mobile-dark.png"), FullPage = true });

        await using var noJs = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            ViewportSize = new() { Width = 390, Height = 844 },
        });
        var staticPage = await noJs.NewPageAsync();
        await staticPage.GotoAsync(baseUrl);
        await Expect(staticPage.Locator("[data-resource]:visible")).ToHaveCountAsync(6);
        await Expect(staticPage.Locator(".library-tools")).ToBeHiddenAsync();
        await staticPage.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Link, new() { Name = "Plan your project" }).ClickAsync();
        await Expect(staticPage.GetByRole(AriaRole.Heading, new() { Name = "Plan your project." })).ToBeInViewportAsync();
    });
}

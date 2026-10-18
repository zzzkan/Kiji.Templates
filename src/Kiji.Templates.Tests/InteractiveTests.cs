using Microsoft.Playwright;
using Xunit;

namespace Kiji.Templates.Tests;

public class InteractiveTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ViewportSize = new() { Width = 1280, Height = 800 },
        Locale = "en-US",
        TimezoneId = "America/Los_Angeles",
    };

    [Theory]
    [InlineData(false, "/")]
    [InlineData(true, "/Kiji.Templates/interactive/")]
    public Task PublishedInteractiveWorks(bool subpath, string prefix)
        => VerifySite("interactive", subpath, async (baseUrl, diagnostics) =>
    {
        await Page.GotoAsync(baseUrl);
        await ExpectPageHead("Home", "Familiar Blazor template pages with JavaScript interactions.", $"https://example.com{prefix}");
        await Expect(Page.GetByRole(AriaRole.Main)).ToMatchAriaSnapshotAsync("""
            - main:
              - /children: deep-equal
              - link "About":
                - /url: https://github.com/zzzkan/kiji
              - article:
                - heading "Hello, world!" [level=1]
                - text: Welcome to your new app.
            """);
        await File.WriteAllTextAsync(Path.Combine(diagnostics, "home.aria.yml"),
            await Page.GetByRole(AriaRole.Main).AriaSnapshotAsync(), TestContext.Current.CancellationToken);
        var navigation = Page.GetByRole(AriaRole.Navigation);
        await Expect(navigation).ToMatchAriaSnapshotAsync($"""
            - navigation "Main navigation":
              - /children: equal
              - link "Home":
                - /url: {prefix}
              - link "Counter":
                - /url: {prefix}counter/
              - link "Weather":
                - /url: {prefix}weather/
            """);
        await File.WriteAllTextAsync(Path.Combine(diagnostics, "navigation.aria.yml"),
            await navigation.AriaSnapshotAsync(), TestContext.Current.CancellationToken);
        await Expect(navigation.GetByRole(AriaRole.Link, new() { Name = "Home", Exact = true }))
            .ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Navigation menu" })).ToBeHiddenAsync();
        // Bootstrap must survive both package exclusions and Kiji's asset pipeline.
        await Expect(Page.Locator(".sidebar")).ToHaveCSSAsync("width", "250px");
        await Expect(Page.Locator(".navbar-toggler-icon")).ToHaveCSSAsync("width", "30px");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "home-desktop.png"), FullPage = true });
        await Page.GetByRole(AriaRole.Link, new() { Name = "Counter", Exact = true }).ClickAsync();
        await ExpectPageHead("Counter", "A simple interactive counter.", $"https://example.com{prefix}counter/");
        var increment = Page.GetByRole(AriaRole.Button, new() { Name = "Click me" });
        await Expect(increment).ToHaveCSSAsync("background-color", "rgb(27, 110, 194)");
        await increment.ClickAsync();
        await increment.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Current count: 2");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "counter-desktop.png"), FullPage = true });

        var weather = Page.GetByRole(AriaRole.Link, new() { Name = "Weather", Exact = true });
        await weather.ClickAsync();
        await ExpectPageHead("Weather", "Weather forecasts loaded from sample data.", $"https://example.com{prefix}weather/");
        await Expect(Page.GetByRole(AriaRole.Table)).ToMatchAriaSnapshotAsync("""
            - table "Weather forecast":
              - /children: equal
              - caption: Weather forecast
              - rowgroup:
                - row:
                  - /children: equal
                  - columnheader "Date"
                  - columnheader "Temperature in Celsius"
                  - columnheader "Temperature in Fahrenheit"
                  - columnheader "Summary"
              - rowgroup:
                - /children: equal
                - row "1/6/2022 1 33 Freezing"
                - row "1/7/2022 14 57 Bracing"
                - row "1/8/2022 -13 9 Freezing"
                - row "1/9/2022 -16 4 Balmy"
                - row "1/10/2022 -2 29 Chilly"
            """);
        await File.WriteAllTextAsync(Path.Combine(diagnostics, "weather.aria.yml"),
            await Page.GetByRole(AriaRole.Table).AriaSnapshotAsync(), TestContext.Current.CancellationToken);
        await Expect(Page.GetByRole(AriaRole.Status)).ToBeHiddenAsync();
        await Expect(weather).ToHaveAttributeAsync("aria-current", "page");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "desktop.png"), FullPage = true });

        await Page.SetViewportSizeAsync(390, 844);
        await Page.GotoAsync(baseUrl);
        var toggle = Page.GetByRole(AriaRole.Button, new() { Name = "Navigation menu" });
        await Expect(navigation).ToBeHiddenAsync();
        await Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "mobile-collapsed.png"), FullPage = true });
        await toggle.ClickAsync();
        await Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(navigation).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "mobile.png"), FullPage = true });
        await toggle.ClickAsync();
        await Expect(navigation).ToBeHiddenAsync();

        await Page.SetViewportSizeAsync(641, 844);
        await Expect(navigation).ToBeVisibleAsync();
        await Expect(toggle).ToBeHiddenAsync();
        await Page.SetViewportSizeAsync(640, 844);
        await Expect(navigation).ToBeHiddenAsync();
        await toggle.FocusAsync();
        await Page.Keyboard.PressAsync("Space");
        await Expect(navigation).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Counter", Exact = true }).ClickAsync();
        await Expect(navigation).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Current count: 0");

        var notices = await Page.APIRequest.GetAsync($"{baseUrl}THIRD-PARTY-NOTICES.txt");
        Assert.True(notices.Ok);
        Assert.Contains("Copyright (c) .NET Foundation and Contributors", await notices.TextAsync());
        Assert.Contains("Copyright (c) 2019-2024 The Bootstrap Authors", await notices.TextAsync());

        await Page.GotoAsync($"{baseUrl}404.html");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Not Found");

        await using var noJs = await Browser.NewContextAsync(new()
        {
            JavaScriptEnabled = false,
            ViewportSize = new() { Width = 390, Height = 844 },
        });
        var staticPage = await noJs.NewPageAsync();
        await staticPage.GotoAsync(baseUrl);
        await Expect(staticPage.GetByRole(AriaRole.Navigation)).ToBeVisibleAsync();
        await Expect(staticPage.GetByRole(AriaRole.Button, new() { Name = "Navigation menu" })).ToBeHiddenAsync();
    });

    private async Task ExpectPageHead(string title, string description, string canonicalUrl)
    {
        await Expect(Page).ToHaveTitleAsync(title);
        await Expect(Page.Locator("head title")).ToHaveCountAsync(1);
        await Expect(Page.Locator("head meta[charset]")).ToHaveAttributeAsync("charset", "utf-8");
        await Expect(Page.Locator("head meta[name=viewport]")).ToHaveAttributeAsync("content", "width=device-width, initial-scale=1");
        await Expect(Page.Locator("head link[rel=stylesheet]")).ToHaveCountAsync(3);
        await Expect(Page.Locator("head link[rel=icon]")).ToHaveCountAsync(1);
        await Expect(Page.Locator("head meta[name=description]")).ToHaveAttributeAsync("content", description);
        await Expect(Page.Locator("head link[rel=canonical]")).ToHaveAttributeAsync("href", canonicalUrl);
        await Expect(Page.Locator("head meta[property='og:title']")).ToHaveAttributeAsync("content", title);
        await Expect(Page.Locator("head meta[property='og:description']")).ToHaveAttributeAsync("content", description);
        await Expect(Page.Locator("head meta[property='og:url']")).ToHaveAttributeAsync("content", canonicalUrl);
        await Expect(Page.Locator("head meta[property='og:type']")).ToHaveAttributeAsync("content", "website");
    }
}

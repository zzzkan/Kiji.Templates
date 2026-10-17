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
        await Expect(Page.GetByRole(AriaRole.Main)).ToMatchAriaSnapshotAsync("""
            - main:
              - /children: deep-equal
              - heading "Hello, world!" [level=1]
              - paragraph: Welcome to your new app.
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
        await Page.GetByRole(AriaRole.Link, new() { Name = "Counter", Exact = true }).ClickAsync();
        var increment = Page.GetByRole(AriaRole.Button, new() { Name = "Click me" });
        await increment.ClickAsync();
        await increment.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Current count: 2");

        var weather = Page.GetByRole(AriaRole.Link, new() { Name = "Weather", Exact = true });
        await weather.ClickAsync();
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
        await toggle.ClickAsync();
        await Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(navigation).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "mobile.png"), FullPage = true });
        await toggle.ClickAsync();
        await Expect(navigation).ToBeHiddenAsync();

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
    });
}

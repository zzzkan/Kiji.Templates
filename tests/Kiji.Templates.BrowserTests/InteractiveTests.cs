using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Kiji.Templates.BrowserTests;

public class InteractiveTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ViewportSize = new() { Width = 1280, Height = 800 },
        Locale = "en-US",
        TimezoneId = "America/Los_Angeles",
    };

    [Theory]
    [InlineData("MySite/dist", "/", "root")]
    [InlineData("subpath", "/Kiji.Templates/interactive/", "subpath")]
    public async Task PublishedInteractiveWorks(string siteDirectory, string prefix, string caseName)
    {
        var run = Environment.GetEnvironmentVariable("KIJI_VERIFICATION_DIRECTORY")
            ?? throw new InvalidOperationException("Set KIJI_VERIFICATION_DIRECTORY to the verification output directory.");
        var root = Path.GetFullPath(Path.Combine(run, "sites", "interactive", siteDirectory));
        var diagnostics = Path.Combine(run, "logs", caseName);
        Directory.CreateDirectory(diagnostics);

        using var files = new PhysicalFileProvider(root);
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        await using var app = builder.Build();
        var requestPath = prefix.TrimEnd('/');
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files, RequestPath = requestPath });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = files, RequestPath = requestPath });
        await app.StartAsync(TestContext.Current.CancellationToken);

        try
        {
            var baseUrl = app.Urls.Single() + prefix;
            Page.SetDefaultTimeout(10_000);
            var errors = new ConcurrentQueue<string>();
            Page.PageError += (_, error) => errors.Enqueue(error);
            Page.Response += (_, response) =>
            {
                if (response.Status >= 400) errors.Enqueue($"{response.Status} {response.Url}");
            };

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
            Assert.Empty(errors);

            await using var noJs = await Browser.NewContextAsync(new()
            {
                JavaScriptEnabled = false,
                ViewportSize = new() { Width = 390, Height = 844 },
            });
            var staticPage = await noJs.NewPageAsync();
            await staticPage.GotoAsync(baseUrl);
            await Expect(staticPage.GetByRole(AriaRole.Navigation)).ToBeVisibleAsync();
        }
        catch
        {
            try
            {
                await Page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "failure.png"), FullPage = true });
            }
            catch (PlaywrightException)
            {
                // Preserve the test failure if the browser has already closed.
            }
            throw;
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }
}

using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public abstract class PublishedSiteTest(TemplateFixture fixture) : PageTest
{
    public override Task<BrowserTypeLaunchOptions?> LaunchOptionsAsync()
    {
        var channel = Environment.GetEnvironmentVariable("KIJI_TEST_BROWSER_CHANNEL");
        if (string.IsNullOrEmpty(channel))
            return base.LaunchOptionsAsync();

        var options = new BrowserTypeLaunchOptions { Channel = channel };
        // Edge can retain the OS locale for Intl despite the context's Locale.
        if (channel == "msedge" && ContextOptions().Locale is { } locale)
            options.Args = [$"--lang={locale}"];
        return Task.FromResult<BrowserTypeLaunchOptions?>(options);
    }

    protected async Task VerifySite(string template, bool subpath,
        Func<string, string, Task> verify)
    {
        var site = await fixture.GetSite(template, subpath);
        var prefix = site.Prefix;
        var diagnostics = Path.Combine(fixture.Logs, template, subpath ? "subpath" : "root");
        Directory.CreateDirectory(diagnostics);

        using var files = new PhysicalFileProvider(site.Output);
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        await using var app = builder.Build();
        app.UseFileServer(new FileServerOptions { FileProvider = files, RequestPath = prefix.TrimEnd('/') });
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
            Page.RequestFailed += (_, request) => errors.Enqueue($"{request.Failure} {request.Url}");
            Page.Request += (_, request) =>
            {
                var isPrism = template == "blog"
                    && request.Url.StartsWith("https://cdn.jsdelivr.net/npm/prismjs@1.30.0/", StringComparison.Ordinal);
                if (!request.Url.StartsWith(baseUrl, StringComparison.Ordinal) && !isPrism)
                    errors.Enqueue($"Unexpected external request: {request.Url}");
            };

            await verify(baseUrl, diagnostics);
            Assert.Empty(errors);
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

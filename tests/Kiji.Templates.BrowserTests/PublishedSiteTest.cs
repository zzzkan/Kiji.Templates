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

public abstract class PublishedSiteTest : PageTest
{
    protected async Task VerifySite(string template, string siteDirectory, string prefix, string caseName,
        Func<string, string, Task> verify)
    {
        var run = Environment.GetEnvironmentVariable("KIJI_VERIFICATION_DIRECTORY")
            ?? throw new InvalidOperationException("Set KIJI_VERIFICATION_DIRECTORY to the verification output directory.");
        var root = Path.GetFullPath(Path.Combine(run, "sites", template, siteDirectory));
        var diagnostics = Path.Combine(run, "logs", template, caseName);
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
            Page.RequestFailed += (_, request) => errors.Enqueue($"{request.Failure} {request.Url}");
            Page.Request += (_, request) =>
            {
                if (!request.Url.StartsWith(baseUrl, StringComparison.Ordinal))
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

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace Kiji.Templates.Tests;

[Collection("Templates")]
public sealed class TemplateParameterTests(TemplateFixture fixture) : IDisposable
{
    public void Dispose() => fixture.RecordTestResult();

    public static TheoryData<string> TemplateKinds => new(TemplateFixture.Templates.Select(template => template.Kind));

    [Theory]
    [MemberData(nameof(TemplateKinds))]
    public async Task SiteInfoParametersPreserveTextAndProjectName(string kind)
    {
        const string siteName = "旅の \"Journal\" \\ Notes";
        const string description = "Notes with \"quotes\", a backslash \\ and a newline\nSecond line.";
        const string author = "Aki \"Kiji\" \\ Writer";
        var site = await fixture.GenerateSite(kind, subpath: true, scenario: "site-info", siteInfo: new Dictionary<string, string>
        {
            ["--site-name"] = siteName,
            ["--site-description"] = description,
            ["--site-language"] = "ja",
            ["--site-author"] = author,
        });
        var program = await File.ReadAllTextAsync(Path.Combine(site.Project, "Program.cs"), TestContext.Current.CancellationToken);
        var jsonOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        foreach (var (property, value) in new[]
            { ("Name", siteName), ("Description", description), ("Language", "ja"), ("Author", author) })
            Assert.Contains($"{property} = {JsonSerializer.Serialize(value, jsonOptions)}", program);

        await fixture.Publish(site);
        var html = await File.ReadAllTextAsync(Path.Combine(site.Output, "index.html"), TestContext.Current.CancellationToken);
        Assert.Contains("lang=\"ja\"", html);
        if (kind == "blog")
        {
            var feed = XDocument.Load(Path.Combine(site.Output, "feed.xml"));
            var channel = feed.Root!.Element("channel")!;
            Assert.Equal(siteName, channel.Element("title")!.Value);
            Assert.Equal(description, channel.Element("description")!.Value);
            Assert.Equal("ja", channel.Element("language")!.Value);
        }
    }
}

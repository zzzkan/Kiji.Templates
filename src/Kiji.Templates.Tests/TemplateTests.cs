using Xunit;

namespace Kiji.Templates.Tests;

public sealed class TemplateTests(TemplateFixture fixture) : PublishedSiteTest(fixture)
{
    public static TheoryData<string, bool> Templates
    {
        get
        {
            var cases = new TheoryData<string, bool>();
            foreach (var template in TemplateFixture.Package.Templates)
            {
                cases.Add(template.Kind, false);
                cases.Add(template.Kind, true);
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public Task GeneratedSiteLoads(string kind, bool subpath) => VerifySite(kind, subpath, async (baseUrl, _) =>
    {
        await Page.GotoAsync(baseUrl);
        Assert.False(string.IsNullOrWhiteSpace(await Page.Locator("body").InnerTextAsync()));
    });
}

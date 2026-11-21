using Kiji;
using Microsoft.Extensions.Hosting;

var builder = StaticSite.CreateBuilder(args);
builder.UseSiteInfo(new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
});

await builder.Build().RunAsync();

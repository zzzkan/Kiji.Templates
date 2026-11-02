using Kiji;
using Kiji.Interactive.Layout;
using Kiji.Interactive.Pages;

var builder = StaticSite.CreateBuilder(args);
builder.SiteInfo = new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
};

builder.UseDefaultLayout<MainLayout>();

builder.UseNotFoundPage<NotFound>();

return await builder.Build().RunAsync();

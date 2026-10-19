using Kiji;
using Kiji.Interactive.Layout;
using Kiji.Interactive.Pages;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
};

app.UseDefaultLayout<MainLayout>();

app.AddPages();
app.AddNotFoundPage<NotFound>();

return await app.RunAsync();

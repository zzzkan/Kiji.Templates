using Kiji;
using Kiji.Interactive.Layout;
using Kiji.Interactive.Pages;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Kiji.Interactive",
    Description = "Familiar Blazor template pages with JavaScript interactions.",
    Language = "en",
    Author = "zzzkan",
};

app.UseDefaultLayout<MainLayout>();

app.AddPages();
app.AddNotFoundPage<NotFound>();

return await app.RunAsync();

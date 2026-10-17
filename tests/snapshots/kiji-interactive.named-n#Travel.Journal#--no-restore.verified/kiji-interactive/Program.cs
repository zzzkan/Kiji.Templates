using Kiji;
using Travel.Journal.Layout;
using Travel.Journal.Pages;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Travel.Journal",
};
app.UseDefaultLayout<MainLayout>();
app.AddPages();
app.AddNotFoundPage<NotFound>();

return await app.RunAsync();

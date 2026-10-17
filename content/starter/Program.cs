using Kiji;
using Kiji.Starter.Layout;
using Kiji.Starter.Pages;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Kiji.Starter",
};
app.UseDefaultLayout<MainLayout>();
app.AddPages();
app.AddNotFoundPage<NotFound>();

return await app.RunAsync();

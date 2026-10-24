using Kiji;
using Microsoft.FluentUI.AspNetCore.Components;
using Kiji.Fluent.Layout;
using Kiji.Fluent.Pages;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
};

app.UseServices(services => services.AddFluentUIComponents());

app.UseDefaultLayout<MainLayout>();

app.AddNotFoundPage<NotFound>();

return await app.RunAsync();

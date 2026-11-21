using Kiji;
using Microsoft.FluentUI.AspNetCore.Components;
using Kiji.Fluent.Layout;
using Kiji.Fluent.Pages;
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

builder.Services.AddFluentUIComponents();
builder.UseDefaultLayout<MainLayout>();
builder.UseNotFoundPage<NotFound>();

await builder.Build().RunAsync();

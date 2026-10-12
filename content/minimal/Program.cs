using Kiji;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "KijiSite",
};
app.AddPages();

return await app.RunAsync();

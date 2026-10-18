using Kiji;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Kiji.Minimal",
    Description = "A minimal site with a single home page.",
    Language = "en",
    Author = "zzzkan",
};

app.AddPages();

return await app.RunAsync();

using Kiji;
using Travel.Journal.Layout;
using Travel.Journal.Models;
using Travel.Journal.Pages;
using Kiji.Feeds;
using Kiji.Markdown;
using Microsoft.Extensions.DependencyInjection;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Travel.Journal",
    Description = "Notes on things made, read, and noticed.",
    Language = "en",
};
app.UseMarkdownContent<FrontMatter, BlogPost>(BlogPost.Create);
app.UseDefaultLayout<MainLayout>();
app.AddPages();
app.AddPages<Post>(services => services.GetRequiredService<ContentDictionary<BlogPost>>()
    .Select(post => new { Slug = post.Value.Slug, ContentKey = post.Key }));
app.AddNotFoundPage<NotFound>();
app.AddRssFeed(services => services.GetRequiredService<ContentDictionary<BlogPost>>().Values
    .OrderByDescending(post => post.Date)
    .ThenBy(post => post.Slug, StringComparer.Ordinal)
    .Select(post => new FeedItem(post.Title, post.Description, post.Date, post.Path)));

return await app.RunAsync();

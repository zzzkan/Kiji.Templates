using Kiji;
using Kiji.Blog.Layout;
using Kiji.Blog.Models;
using Kiji.Blog.Pages;
using Kiji.Feeds;
using Kiji.Markdown;
using Microsoft.Extensions.DependencyInjection;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("https://example.com/"),
    Name = "Kiji.Blog",
    Description = "A Markdown blog with RSS and light/dark colors.",
    Language = "en",
    Author = "zzzkan",
};

app.UseMarkdownContent<PostFrontMatter, BlogPost>(BlogPost.Create,
    options => options.Directory = "content/posts");
app.UseMarkdownContent<PageFrontMatter, SitePage>(SitePage.Create,
    options => options.Directory = "content/pages");
app.UseDefaultLayout<MainLayout>();

app.AddPages();
app.AddPages<Post>(services => services.GetRequiredService<ContentDictionary<BlogPost>>()
    .Select(post => new { Slug = post.Value.Slug, ContentKey = post.Key }));
app.AddPages<Page>(services => services.GetRequiredService<ContentDictionary<SitePage>>()
    .Select(page => new { Slug = page.Value.Slug, ContentKey = page.Key }));
app.AddNotFoundPage<NotFound>();

app.AddRssFeed(services => services.GetRequiredService<ContentDictionary<BlogPost>>().Values
    .OrderByDescending(post => post.Date)
    .ThenBy(post => post.Slug, StringComparer.Ordinal)
    .Select(post => new FeedItem(post.Title, post.Description, post.Date, post.RelativePath)));

return await app.RunAsync();

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
    Description = "Notes on things made, read, and noticed.",
    Language = "en",
};

app.UseMarkdownContent<PostFrontMatter, BlogPost>(BlogPost.Create,
    options => options.Directory = "content/posts");
app.UseDefaultLayout<MainLayout>();

app.AddPages();
app.AddPages<Post>(services => services.GetRequiredService<ContentDictionary<BlogPost>>()
    .Select(post => new { Slug = post.Value.Slug, ContentKey = post.Key }));
app.AddNotFoundPage<NotFound>();

app.AddRssFeed(services => services.GetRequiredService<ContentDictionary<BlogPost>>().Values
    .OrderByDescending(post => post.Date)
    .ThenBy(post => post.Slug, StringComparer.Ordinal)
    .Select(post => new FeedItem(post.Title, post.Description, post.Date, post.RelativePath)));

return await app.RunAsync();

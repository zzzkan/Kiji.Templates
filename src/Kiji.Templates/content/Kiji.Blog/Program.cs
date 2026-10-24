using Kiji;
using Kiji.Blog.Layout;
using Kiji.Blog.Models;
using Kiji.Blog.Pages;
using Kiji.Feeds;
using Kiji.Markdown;
using Kiji.Sitemaps;
using Microsoft.Extensions.DependencyInjection;

var app = StaticSite.Create(args);
app.Info = new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
};

app.UseMarkdownContent<PostFrontMatter, BlogPost>(BlogPost.Create,
    options => options.Directory = "content/posts");
app.UseMarkdownContent<PageFrontMatter, SitePage>(SitePage.Create,
    options => options.Directory = "content/pages");
app.UseDefaultLayout<MainLayout>();

app.AddPageParameters<Post>(services => services.GetRequiredService<ContentDictionary<BlogPost>>()
    .Select(post => new { Slug = post.Value.Slug, ContentKey = post.Key }));
app.AddPageParameters<Page>(services => services.GetRequiredService<ContentDictionary<SitePage>>()
    .Select(page => new { Slug = page.Value.Slug, ContentKey = page.Key }));
app.AddNotFoundPage<NotFound>();
app.AddSitemap();

app.AddRssFeed(services => services.GetRequiredService<ContentDictionary<BlogPost>>().Values
    .OrderByDescending(post => post.Date)
    .ThenBy(post => post.Slug, StringComparer.Ordinal)
    .Select(post => new FeedItem(post.Title, post.Description, post.Date, post.RelativePath)));

return await app.RunAsync();

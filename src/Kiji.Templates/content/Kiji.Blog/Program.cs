using Kiji;
using Kiji.Blog.Layout;
using Kiji.Blog.Models;
using Kiji.Blog.Pages;
using Kiji.Feeds;
using Kiji.Markdown;
using Kiji.Sitemaps;
using Microsoft.Extensions.DependencyInjection;

var builder = StaticSite.CreateBuilder(args);
builder.SiteInfo = new SiteInfo
{
    BaseUrl = new Uri("__KIJI_BASE_URL__"),
    Name = "__KIJI_SITE_NAME__",
    Description = "__KIJI_DESCRIPTION__",
    Language = "__KIJI_SITE_LANGUAGE__",
    Author = "__KIJI_SITE_AUTHOR__",
};

builder.AddMarkdownContent<PostFrontMatter, BlogPost>(BlogPost.Create,
    options => options.Directory = "content/posts");
builder.AddMarkdownContent<PageFrontMatter, SitePage>(SitePage.Create,
    options => options.Directory = "content/pages");
builder.UseDefaultLayout<MainLayout>();

builder.AddPageParameters<Post>(services => services.GetRequiredService<ContentCollection<BlogPost>>()
    .Select(post => new { post.Slug, PostContent = post }));
builder.AddPageParameters<Page>(services => services.GetRequiredService<ContentCollection<SitePage>>()
    .Select(page => new { page.Slug, PageContent = page }));
builder.UseNotFoundPage<NotFound>();
builder.AddSitemap();

builder.AddRssFeed(services => services.GetRequiredService<ContentCollection<BlogPost>>()
    .OrderByDescending(post => post.Content.FrontMatter.Date)
    .ThenBy(post => post.Slug, StringComparer.Ordinal)
    .Select(post => new FeedItem(post.Content.FrontMatter.Title, post.Content.FrontMatter.Description,
        post.Content.FrontMatter.Date, Post.PathFor(post.Slug))));

return await builder.Build().RunAsync();

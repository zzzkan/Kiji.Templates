using Kiji;
using Kiji.Doc.Layout;
using Kiji.Doc.Models;
using Kiji.Doc.Pages;
using Kiji.Markdown;
using Kiji.Sitemaps;
using Microsoft.Extensions.DependencyInjection;
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

builder.AddMarkdownContent<FrontMatter, Article>(Article.Create);
builder.UseDefaultLayout<MainLayout>();
builder.UseNotFoundPage<NotFoundPage>();
builder.AddPageParameters<ArticlePage>(services => services.GetRequiredService<ContentCollection<Article>>()
    .Select(article => new { article.Slug, Article = article }));
builder.AddSitemap();

await builder.Build().RunAsync();

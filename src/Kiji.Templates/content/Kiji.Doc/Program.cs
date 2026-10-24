using Kiji;
using Kiji.Doc.Layout;
using Kiji.Doc.Models;
using Kiji.Doc.Pages;
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

app.UseMarkdownContent<FrontMatter, Article>(Article.Create);
app.UseDefaultLayout<MainLayout>();
app.AddPageParameters<ArticlePage>(services => services.GetRequiredService<ContentDictionary<Article>>()
    .Select(article => new { article.Value.Slug, ContentKey = article.Key }));
app.AddNotFoundPage<NotFoundPage>();
app.AddSitemap();

return await app.RunAsync();

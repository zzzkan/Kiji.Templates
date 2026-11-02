using Kiji.Markdown;

namespace Kiji.Blog.Models;

public sealed record SitePage(
    string Slug, string Title, string Description, MarkdownContent<PageFrontMatter> Content)
{
    public static SitePage Create(MarkdownContent<PageFrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title))
        {
            throw new InvalidOperationException($"{content.Path}: pages require a title.");
        }

        return new SitePage(ContentSlug.FromPath(content.Path), content.FrontMatter.Title,
            content.FrontMatter.Description, content);
    }
}

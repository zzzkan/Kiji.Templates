using Kiji.Markdown;

namespace Kiji.Blog.Models;

public sealed record SitePage(
    string Slug, string Title, string Description, MarkdownContent<PageFrontMatter> Content)
{
    public static SitePage Create(MarkdownContent<PageFrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title))
        {
            throw new InvalidOperationException($"{content.FileInfo.FullName}: pages require a title.");
        }

        var slug = string.Equals(content.FileInfo.Name, "index.md", StringComparison.OrdinalIgnoreCase)
            ? content.FileInfo.Directory!.Name
            : Path.GetFileNameWithoutExtension(content.FileInfo.Name);

        return new SitePage(slug, content.FrontMatter.Title, content.FrontMatter.Description, content);
    }
}

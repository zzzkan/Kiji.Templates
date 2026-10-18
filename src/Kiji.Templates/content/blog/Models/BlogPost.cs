using Kiji.Markdown;

namespace Kiji.Blog.Models;

public sealed record BlogPost(
    string Slug, string Title, string Description, DateTimeOffset Date,
    MarkdownContent<PostFrontMatter> Content)
{
    public string RelativePath => $"posts/{Uri.EscapeDataString(Slug)}/";

    public static BlogPost Create(MarkdownContent<PostFrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title) || content.FrontMatter.Date is null)
        {
            throw new InvalidOperationException($"{content.FileInfo.FullName}: posts require a title and date.");
        }

        var slug = string.Equals(content.FileInfo.Name, "index.md", StringComparison.OrdinalIgnoreCase)
            ? content.FileInfo.Directory!.Name
            : Path.GetFileNameWithoutExtension(content.FileInfo.Name);

        return new BlogPost(slug, content.FrontMatter.Title,
            content.FrontMatter.Description, content.FrontMatter.Date.Value, content);
    }
}

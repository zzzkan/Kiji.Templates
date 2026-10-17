using Kiji.Markdown;

namespace Kiji.Blog.Models;

public sealed record BlogPost(
    string Slug, string Title, string Description, DateTimeOffset Date,
    MarkdownContent<FrontMatter> Content)
{
    public string Path => $"posts/{Uri.EscapeDataString(Slug)}/";

    public static BlogPost Create(MarkdownContent<FrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title) || content.FrontMatter.Date is null)
        {
            throw new InvalidOperationException($"{content.FileInfo.FullName}: posts require a title and date.");
        }

        return new BlogPost(content.FileInfo.Directory!.Name, content.FrontMatter.Title,
            content.FrontMatter.Description, content.FrontMatter.Date.Value, content);
    }
}

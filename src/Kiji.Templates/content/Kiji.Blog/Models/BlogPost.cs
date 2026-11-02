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
            throw new InvalidOperationException($"{content.Path}: posts require a title and date.");
        }

        return new BlogPost(ContentSlug.FromPath(content.Path), content.FrontMatter.Title,
            content.FrontMatter.Description, content.FrontMatter.Date.Value, content);
    }
}

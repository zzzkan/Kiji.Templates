using Kiji.Markdown;

namespace Kiji.Blog.Models;

public sealed record BlogPost(string Slug, MarkdownContent<PostFrontMatter> Content)
{
    public static BlogPost Create(MarkdownContent<PostFrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title) || content.FrontMatter.Date == default)
        {
            throw new InvalidOperationException($"{content.Path}: posts require a title and date.");
        }

        return new BlogPost(ContentSlug.FromPath(content.Path), content);
    }
}

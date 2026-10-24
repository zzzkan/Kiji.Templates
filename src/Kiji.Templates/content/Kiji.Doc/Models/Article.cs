using Kiji.Markdown;
using System.Text.RegularExpressions;

namespace Kiji.Doc.Models;

public sealed record Article(string Slug, MarkdownContent<FrontMatter> Content)
{
    public string Title => Content.FrontMatter.Title;
    public string Description => Content.FrontMatter.Description;
    public int Order => Content.FrontMatter.Order;
    public string RelativePath => $"docs/{Slug}/";

    public static Article Create(MarkdownContent<FrontMatter> content)
    {
        if (string.IsNullOrWhiteSpace(content.FrontMatter.Title))
            throw new InvalidOperationException($"{content.FileInfo.FullName}: articles require a title.");

        var name = string.Equals(content.FileInfo.Name, "index.md", StringComparison.OrdinalIgnoreCase)
            ? content.FileInfo.Directory!.Name
            : Path.GetFileNameWithoutExtension(content.FileInfo.Name);
        var slug = Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9-]+", "-").Trim('-');
        if (slug.Length == 0)
            throw new InvalidOperationException($"{content.FileInfo.FullName}: use a file or directory name with ASCII letters or digits.");

        return new Article(slug, content);
    }
}

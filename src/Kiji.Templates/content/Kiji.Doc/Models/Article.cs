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
            throw new InvalidOperationException($"{content.Path}: articles require a title.");

        var name = string.Equals(Path.GetFileName(content.Path), "index.md", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(content.Path))!
            : Path.GetFileNameWithoutExtension(content.Path);
        var slug = Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9-]+", "-").Trim('-');
        if (slug.Length == 0)
            throw new InvalidOperationException(
                $"{content.Path}: name the file, or the directory that contains index.md, with ASCII letters or digits.");

        return new Article(slug, content);
    }
}

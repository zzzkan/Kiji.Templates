namespace Kiji.Blog.Models;

public sealed class PostFrontMatter
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset Date { get; init; }
}

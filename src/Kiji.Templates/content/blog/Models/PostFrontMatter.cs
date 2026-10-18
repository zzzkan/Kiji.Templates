namespace Kiji.Blog.Models;

public sealed class PostFrontMatter
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? Date { get; set; }
}

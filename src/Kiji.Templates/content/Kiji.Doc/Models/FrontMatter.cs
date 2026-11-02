namespace Kiji.Doc.Models;

public sealed class FrontMatter
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Order { get; init; }
}

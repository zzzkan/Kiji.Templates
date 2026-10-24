namespace Kiji.Doc.Models;

public sealed class FrontMatter
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

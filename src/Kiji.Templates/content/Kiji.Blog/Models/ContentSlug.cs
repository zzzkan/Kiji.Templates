namespace Kiji.Blog.Models;

internal static class ContentSlug
{
    public static string FromPath(string path)
    {
        var slug = string.Equals(Path.GetFileName(path), "index.md", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileName(Path.GetDirectoryName(path))
            : Path.GetFileNameWithoutExtension(path);

        return string.IsNullOrEmpty(slug)
            ? throw new InvalidOperationException($"{path}: name the file, or the directory that contains index.md.")
            : slug;
    }
}

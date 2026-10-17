using System.IO.Compression;
using System.Text.Json;

namespace Kiji.Templates.Infrastructure;

internal sealed record Template(string Kind, string ShortName, string Identity, string SourceName);

internal sealed class TemplatePackage
{
    public string Path { get; }
    public Template[] Templates { get; }

    public TemplatePackage(string path)
    {
        Path = System.IO.Path.GetFullPath(Directory.Exists(path)
            ? Directory.GetFiles(path, "*.nupkg").Single()
            : path);
        using var archive = ZipFile.OpenRead(Path);
        Templates = archive.Entries
            .Where(entry => entry.FullName.Split('/') is ["content", _, ".template.config", "template.json"])
            .Select(entry =>
            {
                using var stream = entry.Open();
                using var json = JsonDocument.Parse(stream);
                var definition = json.RootElement;
                return new Template(entry.FullName.Split('/')[1],
                    definition.GetProperty("shortName").GetString()!,
                    definition.GetProperty("identity").GetString()!,
                    definition.GetProperty("sourceName").GetString()!);
            })
            .OrderBy(template => template.Kind, StringComparer.Ordinal)
            .ToArray();
        if (Templates.Length == 0)
            throw new InvalidOperationException($"No templates found in {Path}.");
    }
}

namespace Kiji.Fluent.Models;

public sealed record Resource(string Title, string Category, string Description, string Url, string Number)
{
    public static IReadOnlyList<Resource> All { get; } =
    [
        new("Start with a good foundation", "Build",
            "Create your first Kiji site, add a page, and make it your own.",
            "https://kiji-docs.zzzkan.workers.dev/docs/getting-started/", "01"),
        new("Find your Fluent", "Design",
            "Explore a library of thoughtful components, from buttons to badges.",
            "https://www.fluentui-blazor.net/", "02"),
        new("Build once. Use everywhere.", "Build",
            "Bring reusable Razor components and their styles into your next project.",
            "https://learn.microsoft.com/aspnet/core/blazor/components/class-libraries", "03"),
        new("Give every detail a purpose", "Design",
            "Make clear, inclusive experiences with the Fluent design system.",
            "https://fluent2.microsoft.design/", "04"),
        new("A home for your next idea", "Publish",
            "Take a static site from your repository to the web with GitHub Pages.",
            "https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages", "05"),
        new("Go behind the pages", "Publish",
            "Explore Kiji's source and discover how Razor becomes a static website.",
            "https://github.com/zzzkan/kiji", "06"),
    ];
}

namespace Kiji.Fluent.Models;

public sealed record Resource(string Title, string Category, string Description, string Url, string Emoji)
{
    public static IReadOnlyList<Resource> All { get; } =
    [
        new("A foundation for your digital presence", "Build",
            "Plan your first Kiji site and create a structure your team can build on.",
            "https://kiji-docs.zzzkan.workers.dev/docs/getting-started/", "🏗️"),
        new("A consistent brand experience", "Design",
            "Explore Fluent UI components for a coherent experience across every page.",
            "https://www.fluentui-blazor.net/", "🎨"),
        new("Reusable by design", "Build",
            "Share Razor components and styles across your company's digital projects.",
            "https://learn.microsoft.com/aspnet/core/blazor/components/class-libraries", "🧩"),
        new("Design for every audience", "Design",
            "Create clear, inclusive experiences with the Fluent design system.",
            "https://fluent2.microsoft.design/", "🤝"),
        new("From repository to public website", "Publish",
            "Explore a straightforward static publishing workflow with GitHub Pages.",
            "https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages", "🌐"),
        new("An open foundation for your team", "Publish",
            "Explore Kiji's source and understand the technology behind your website.",
            "https://github.com/zzzkan/kiji", "📖"),
    ];
}

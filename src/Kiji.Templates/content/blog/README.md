# Kiji.Blog

A Markdown blog with dated posts in `content/posts/` and general pages in
`content/pages/`. Includes RSS, a generated `sitemap.xml`, inline images,
Prism syntax highlighting via CDN, and automatic light/dark colors.

Run `dotnet watch` to start a local preview at <http://localhost:8080>.
The site rebuilds when you edit files. Press `Ctrl+C` to stop.

To publish, set `BaseUrl` in `Program.cs` to your site's public URL, then run
`dotnet publish -c Release`. Upload the contents of `dist/` to your static host;
no .NET server is needed.

For more about creating pages and customizing your site, see the
[Kiji documentation](https://kiji-docs.zzzkan.workers.dev/).

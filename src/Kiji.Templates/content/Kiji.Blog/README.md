# Kiji.Blog

A Markdown blog with dated posts in `content/posts/` and general pages in
`content/pages/`. It includes RSS, a generated `sitemap.xml`, inline images,
Prism syntax highlighting via CDN, and automatic light/dark colors.

Create a site:

```sh
dotnet new kiji-blog -o MyBlog --base-url https://example.org/ --site-name "My Blog" --site-description "My personal site." --site-language en --site-author "Your Name"
cd MyBlog
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Blog` |
| `--site-description` | Site description. | `A Markdown blog with RSS and light/dark colors.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

The template writes these values to `SiteInfo` in `Program.cs`, where you can
change them later.

Run `dotnet watch` to start a local preview at <http://localhost:8080>.
The site rebuilds when you edit files. Press `Ctrl+C` to stop.

To publish, run `dotnet publish -c Release` and upload the contents of `dist/`
to your static host. The site does not need a .NET server.

To learn more about creating pages and customizing your site, see the
[Kiji documentation](https://kiji-docs.zzzkan.workers.dev/).

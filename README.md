# Kiji.Templates

[![NuGet](https://img.shields.io/nuget/v/Kiji.Templates.svg)](https://www.nuget.org/packages/Kiji.Templates)

Create a static website with Razor and [Kiji](https://github.com/zzzkan/kiji).

| Template | Short name | Contents |
| --- | --- | --- |
| Kiji Minimal | `kiji-minimal` | A minimal site with a single home page, ready to preview and publish. |
| Kiji Interactive | `kiji-interactive` | A sample site recreating the familiar Blazor WebAssembly template pages with Razor, Kiji, and JavaScript interactions. |
| Kiji Blog | `kiji-blog` | A small Markdown blog with dated posts, an About page, RSS, and automatic light/dark colors. |

Requires the **.NET 10 SDK**.

## Create and preview a site

```sh
dotnet new install Kiji.Templates
dotnet new kiji-minimal -o MySite
cd MySite
dotnet watch
```

Open <http://localhost:8080>. Edit `Pages/Home.razor` and set your site's name and
published URL in `Program.cs`. The generated README covers working on your site.

Use `-n MySite` to choose a project name independently of its directory.
Dependencies are restored automatically; add `--no-restore` to defer this.

To explore the Interactive sample's Home, Counter, Weather, and Not Found pages,
use `dotnet new kiji-interactive -o MySite`. Customize its interactions in the
`.razor.js` files beside the Razor pages and navigation component.

To start a blog, use `dotnet new kiji-blog -o MyBlog`. Write posts in
`contents/<post-name>/index.md`; the generated README explains front matter,
links, RSS, and customization.

## Publish

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. The published site consists of
static files and does not need a .NET server.

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/)
to add layouts, CSS, more pages, and Markdown content.

## Update or uninstall the templates

```sh
dotnet new update
```

Template updates apply to newly created projects. Existing sites keep their source
files and Kiji package version; update those in each project as needed.

```sh
dotnet new uninstall Kiji.Templates
```

## License

[MIT](https://github.com/zzzkan/Kiji.Templates/blob/main/LICENSE)

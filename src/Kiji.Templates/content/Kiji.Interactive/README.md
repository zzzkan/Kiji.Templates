# Kiji.Interactive

The Home, Counter, and Weather pages follow the .NET 10 Blazor WebAssembly
template and use Bootstrap 5.3.3, Bootstrap Icons, and scoped Razor CSS. Kiji
renders them as static HTML, and the collocated `.razor.js` files handle the
navigation menu, counter, and weather request in the browser.

Create a site:

```sh
dotnet new kiji-interactive -o MySite --base-url https://example.org/ --site-name "My Site" --site-description "My personal site." --site-language en --site-author "Your Name"
cd MySite
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Interactive` |
| `--site-description` | Site description. | `Familiar Blazor template pages with JavaScript interactions.` |
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

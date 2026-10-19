# Kiji.Minimal

Create a site:

```sh
dotnet new kiji-minimal -o MySite --base-url https://example.org/ --site-name "My Site" --site-description "My personal site." --site-language en --site-author "Your Name"
cd MySite
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Minimal` |
| `--site-description` | Site description. | `A minimal site with a single home page.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

These values are written to `SiteInfo` in `Program.cs` and can be edited later.

Run `dotnet watch` to start a local preview at <http://localhost:8080>.
The site rebuilds when you edit files. Press `Ctrl+C` to stop.

To publish, run `dotnet publish -c Release`.
Upload the contents of `dist/` to your static host;
no .NET server is needed.

For more about creating pages and customizing your site, see the
[Kiji documentation](https://kiji-docs.zzzkan.workers.dev/).

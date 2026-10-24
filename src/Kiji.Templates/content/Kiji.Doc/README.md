# Kiji.Doc

Includes an ordered sidebar, a mobile menu that works without JavaScript,
Prism syntax highlighting via CDN, automatic light/dark colors,
page metadata, and a generated sitemap.

```sh
dotnet new kiji-doc -o MyDocs --base-url https://example.org/handbook/ --site-name "My Docs" --site-description "Documentation for my project." --site-language en --site-author "Your Name"
cd MyDocs
dotnet watch
```

The preview opens at <http://localhost:8080>. Press `Ctrl+C` to stop.

| Parameter            | Description                                   | Default                                                          |
| -------------------- | --------------------------------------------- | ---------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                           |
| `--site-name`        | Site display name.                            | `Kiji Doc`                                                       |
| `--site-description` | Site description.                             | `Documentation for your project, built with Markdown and Razor.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                             |
| `--site-author`      | Site author.                                  | `zzzkan`                                                         |

These values are written to `SiteInfo` in `Program.cs` and can be edited later.

Add `content/your-topic/index.md` or `content/your-topic.md`:

```markdown
---
title: Your topic
description: What this page explains.
order: 40
---

## First steps

Write your documentation here.
```

The article publishes at `docs/your-topic/` and appears in the sidebar and home
page automatically. Use unique lowercase ASCII names with digits and hyphens.
The `title` is required. Navigation sorts by `order`, then by slug for ties.

Run `dotnet publish -c Release`, then upload the contents of `dist/` to your
static host. Configure directory URLs to serve `index.html` and use `404.html`
for missing pages. No .NET server is needed.

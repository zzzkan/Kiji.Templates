# Kiji.Doc

A Markdown documentation site with an ordered sidebar, a mobile menu that works
without JavaScript, Prism syntax highlighting via CDN, automatic light/dark
colors, page metadata, and a generated sitemap.

```sh
dotnet new kiji-doc -o MyDocs --base-url https://example.org/handbook/ --site-name "My Docs" --site-description "Documentation for my project." --site-language en --site-author "Your Name"
cd MyDocs
dotnet watch
```

`dotnet watch` starts a local preview at <http://localhost:8080>. Press `Ctrl+C`
to stop.

| Parameter            | Description                                   | Default                                                          |
| -------------------- | --------------------------------------------- | ---------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                           |
| `--site-name`        | Site display name.                            | `Kiji Doc`                                                       |
| `--site-description` | Site description.                             | `Documentation for your project, built with Markdown and Razor.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                             |
| `--site-author`      | Site author.                                  | `zzzkan`                                                         |

The template writes these values to `SiteInfo` in `Program.cs`, where you can
change them later.

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

The article publishes at `docs/your-topic/` and appears in the sidebar and on
the home page automatically. Use unique names made of lowercase ASCII letters,
digits, and hyphens. Every article needs a `title`. Navigation sorts by `order`,
then by slug.

Run `dotnet publish -c Release`, then upload the contents of `dist/` to your
static host. Configure the host to serve `index.html` for directory URLs and
`404.html` for missing pages. The site does not need a .NET server.

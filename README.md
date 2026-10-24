# Kiji.Templates

[![CI](https://github.com/zzzkan/Kiji.Templates/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/zzzkan/Kiji.Templates/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Kiji.Templates.svg)](https://www.nuget.org/packages/Kiji.Templates)
[![License](https://img.shields.io/github/license/zzzkan/Kiji.Templates)](https://github.com/zzzkan/Kiji.Templates/blob/main/LICENSE)

Static website templates built with [Kiji](https://github.com/zzzkan/kiji).

```pwsh
dotnet new install Kiji.Templates
```

| Template                              | Short name         | Contents                                                                                                                                            |
| ------------------------------------- | ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Kiji Minimal](#kiji-minimal)         | `kiji-minimal`     | A minimal site with a single home page.                                                                                                             |
| [Kiji Blog](#kiji-blog)               | `kiji-blog`        | A Markdown blog with posts, general pages, Prism syntax highlighting, and RSS.                                                                      |
| [Kiji Doc](#kiji-doc)                 | `kiji-doc`         | A Markdown documentation site with ordered navigation, syntax highlighting, and responsive light/dark layouts.                                      |
| [Kiji Interactive](#kiji-interactive) | `kiji-interactive` | Home, Counter, and Weather pages from the Blazor WebAssembly template, with JavaScript interactions.                                                |
| [Kiji Fluent](#kiji-fluent)           | `kiji-fluent`      | A corporate website with [Fluent UI Blazor](https://github.com/microsoft/fluentui-blazor), searchable resources, and responsive light/dark layouts. |

## Kiji Minimal

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-minimal/)

A minimal site with a single home page, ready to customize.

![Kiji Minimal screenshot](https://raw.githubusercontent.com/zzzkan/Kiji.Templates/main/images/kiji-minimal.png)

```pwsh
dotnet new kiji-minimal -o MySite `
  --base-url https://example.org/ `
  --site-name "My Site" `
  --site-description "My personal site." `
  --site-language en `
  --site-author "Your Name"
```

| Parameter            | Description                                   | Default                                   |
| -------------------- | --------------------------------------------- | ----------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                    |
| `--site-name`        | Site display name.                            | `Kiji Minimal`                            |
| `--site-description` | Site description.                             | `A minimal site with a single home page.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                      |
| `--site-author`      | Site author.                                  | `zzzkan`                                  |

## Kiji Blog

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-blog/)

A blog with dated posts and About and Policy pages written in Markdown. It also
has inline images, Prism syntax highlighting via CDN, RSS, and automatic
light/dark colors.

![Kiji Blog screenshot](https://raw.githubusercontent.com/zzzkan/Kiji.Templates/main/images/kiji-blog.png)

```pwsh
dotnet new kiji-blog -o MyBlog `
  --base-url https://example.org/blog/ `
  --site-name "My Blog" `
  --site-description "Notes from my days." `
  --site-language en `
  --site-author "Your Name"
```

| Parameter            | Description                                   | Default                                           |
| -------------------- | --------------------------------------------- | ------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                            |
| `--site-name`        | Site display name.                            | `Kiji Blog`                                       |
| `--site-description` | Site description.                             | `A Markdown blog with RSS and light/dark colors.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                              |
| `--site-author`      | Site author.                                  | `zzzkan`                                          |

## Kiji Doc

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-doc/)

A documentation site with Markdown articles, an automatically ordered sidebar
and home page, a mobile menu that works without JavaScript, Prism syntax
highlighting via CDN, light/dark colors, and a sitemap.

![Kiji Doc screenshot](https://raw.githubusercontent.com/zzzkan/Kiji.Templates/main/images/kiji-doc.png)

```pwsh
dotnet new kiji-doc -o MyDocs `
  --base-url https://example.org/handbook/ `
  --site-name "My Docs" `
  --site-description "Documentation for my project." `
  --site-language en `
  --site-author "Your Name"
```

| Parameter            | Description                                   | Default                                                          |
| -------------------- | --------------------------------------------- | ---------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                           |
| `--site-name`        | Site display name.                            | `Kiji Doc`                                                       |
| `--site-description` | Site description.                             | `Documentation for your project, built with Markdown and Razor.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                             |
| `--site-author`      | Site author.                                  | `zzzkan`                                                         |

Add articles as `content/your-topic/index.md` or `content/your-topic.md`, with
`title`, `description`, and `order` in YAML front matter. Articles publish at
`docs/your-topic/`, and navigation sorts them by `order`, then by slug. The
generated README and sample guides cover authoring and deployment.

## Kiji Interactive

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-interactive/)

A sample site inspired by the Blazor WebAssembly template,
with Home, Counter, and Weather pages and JavaScript interactions.

![Kiji Interactive screenshot](https://raw.githubusercontent.com/zzzkan/Kiji.Templates/main/images/kiji-interactive.png)

```pwsh
dotnet new kiji-interactive -o MySite `
  --base-url https://example.org/ `
  --site-name "My Site" `
  --site-description "My interactive site." `
  --site-language en `
  --site-author "Your Name"
```

| Parameter            | Description                                   | Default                                                        |
| -------------------- | --------------------------------------------- | -------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                         |
| `--site-name`        | Site display name.                            | `Kiji Interactive`                                             |
| `--site-description` | Site description.                             | `Familiar Blazor template pages with JavaScript interactions.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                           |
| `--site-author`      | Site author.                                  | `zzzkan`                                                       |

## Kiji Fluent

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-fluent/)

A corporate website with [Fluent UI Blazor](https://github.com/microsoft/fluentui-blazor),
searchable resources, and responsive light/dark layouts.

![Kiji Fluent screenshot](https://raw.githubusercontent.com/zzzkan/Kiji.Templates/main/images/kiji-fluent.png)

```pwsh
dotnet new kiji-fluent -o MyCompany `
  --base-url https://example.org/ `
  --site-name "Your Company" `
  --site-description "A corporate website with Fluent UI components and a searchable resource library." `
  --site-language en `
  --site-author "Your Name"
```

| Parameter            | Description                                   | Default                                                                            |
| -------------------- | --------------------------------------------- | ---------------------------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                                             |
| `--site-name`        | Site display name.                            | `Kiji Fluent`                                                                      |
| `--site-description` | Site description.                             | `A corporate website with Fluent UI components and a searchable resource library.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                                               |
| `--site-author`      | Site author.                                  | `zzzkan`                                                                           |

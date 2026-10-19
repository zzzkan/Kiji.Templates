# Kiji.Templates

[![NuGet](https://img.shields.io/nuget/v/Kiji.Templates.svg)](https://www.nuget.org/packages/Kiji.Templates)

Static website templates built with Razor and [Kiji](https://github.com/zzzkan/kiji).
See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/) for more.

Requires the **.NET 10 SDK**.

```sh
dotnet new install Kiji.Templates
```

| Template                              | Short name         | Contents                                                                                                  |
| ------------------------------------- | ------------------ | --------------------------------------------------------------------------------------------------------- |
| [Kiji Minimal](#kiji-minimal)         | `kiji-minimal`     | A minimal site with a single home page.                                                                   |
| [Kiji Blog](#kiji-blog)               | `kiji-blog`        | A Markdown blog with posts, general pages, Prism syntax highlighting, and RSS, built with Razor and Kiji. |
| [Kiji Interactive](#kiji-interactive) | `kiji-interactive` | Familiar Blazor template pages with JavaScript interactions.                                              |

## Kiji Minimal

A minimal site with a single home page, ready to customize.

```sh
dotnet new kiji-minimal -o MySite --base-url https://example.org/ --site-name "My Site" --site-description "My personal site." --site-language en --site-author "Your Name"
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Minimal` |
| `--site-description` | Site description. | `A minimal site with a single home page.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-minimal/)

<!-- Add a screenshot of the Kiji Minimal demo here. -->

## Kiji Blog

A blog with dated Markdown posts, Markdown About and Policy pages, inline images,
Prism syntax highlighting via CDN, RSS, and automatic light/dark colors.

```sh
dotnet new kiji-blog -o MyBlog --base-url https://example.org/blog/ --site-name "My Blog" --site-description "Notes from my days." --site-language en --site-author "Your Name"
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Blog` |
| `--site-description` | Site description. | `A Markdown blog with RSS and light/dark colors.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-blog/)

<!-- Add a screenshot of the Kiji Blog demo here. -->

## Kiji Interactive

A sample site inspired by the Blazor WebAssembly template,
with Home, Counter, and Weather pages and JavaScript interactions.

```sh
dotnet new kiji-interactive -o MySite --base-url https://example.org/ --site-name "My Site" --site-description "My interactive site." --site-language en --site-author "Your Name"
```

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Interactive` |
| `--site-description` | Site description. | `Familiar Blazor template pages with JavaScript interactions.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

[Demo](https://zzzkan.github.io/Kiji.Templates/kiji-interactive/)

<!-- Add a screenshot of the Kiji Interactive demo here. -->

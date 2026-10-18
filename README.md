# Kiji.Templates

[![NuGet](https://img.shields.io/nuget/v/Kiji.Templates.svg)](https://www.nuget.org/packages/Kiji.Templates)

Static website templates built with Razor and [Kiji](https://github.com/zzzkan/kiji).
See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/) for more.

Requires the **.NET 10 SDK**.

```sh
dotnet new install Kiji.Templates
```

| Template                              | Short name         | Contents                                                     |
| ------------------------------------- | ------------------ | ------------------------------------------------------------ |
| [Kiji Minimal](#kiji-minimal)         | `kiji-minimal`     | A minimal site with a single home page.                      |
| [Kiji Interactive](#kiji-interactive) | `kiji-interactive` | Familiar Blazor template pages with JavaScript interactions. |
| [Kiji Blog](#kiji-blog)               | `kiji-blog`        | A Markdown blog with RSS and light/dark colors.              |

## Kiji Minimal

A minimal site with a single home page, ready to customize.

```sh
dotnet new kiji-minimal -o MySite
```

[Demo](https://zzzkan.github.io/Kiji.Templates/minimal/)

<!-- Add a screenshot of the Kiji Minimal demo here. -->

## Kiji Interactive

A sample site inspired by the Blazor WebAssembly template,
with Home, Counter, and Weather pages and JavaScript interactions.

```sh
dotnet new kiji-interactive -o MySite
```

[Demo](https://zzzkan.github.io/Kiji.Templates/interactive/)

<!-- Add a screenshot of the Kiji Interactive demo here. -->

## Kiji Blog

A Markdown blog with dated posts, an About page, RSS,
and automatic light/dark colors.

```sh
dotnet new kiji-blog -o MyBlog
```

[Demo](https://zzzkan.github.io/Kiji.Templates/blog/)

<!-- Add a screenshot of the Kiji Blog demo here. -->

# Kiji.Templates

[![CI](https://github.com/zzzkan/Kiji.Templates/actions/workflows/ci.yml/badge.svg)](https://github.com/zzzkan/Kiji.Templates/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Kiji.Templates.svg)](https://www.nuget.org/packages/Kiji.Templates)

Create a static website with Razor and [Kiji](https://github.com/zzzkan/kiji).
The `kiji` template gives you a minimal site with a home page, ready to preview and publish.

Requires the **.NET 10 SDK**.

## Create a site

Install the templates and create your project:

```sh
dotnet new install Kiji.Templates
dotnet new kiji -o MySite
cd MySite
```

The project contains:

```text
MySite/
├── MySite.csproj       Razor project with a Kiji package reference
├── Program.cs          Site name, published URL, and page registration
├── Pages/Home.razor    Home page
├── README.md           Instructions for working on your site
└── .gitignore          Excludes build output and local caches
```

To choose a project name independently of its directory, use
`dotnet new kiji -n MySite -o path/to/site`.

## Preview and edit

From your project directory, run:

```sh
dotnet watch
```

Open <http://localhost:8080>. Edit `Pages/Home.razor` to change the home page.
Set your site's name and published URL in `Program.cs`.
Press `Ctrl+C` to stop the preview server.

## Publish

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. The published site consists of
static files and does not need a .NET server.

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/)
to add layouts, CSS, more pages, and Markdown content.

## Update or uninstall the templates

Check for and install updates to your installed templates:

```sh
dotnet new update
```

Template updates apply to newly created projects. Existing sites keep their source
files and Kiji package version; update those in each project as needed.

To remove the templates:

```sh
dotnet new uninstall Kiji.Templates
```

Uninstalling the templates does not remove sites you have already created.

## License

[MIT](https://github.com/zzzkan/Kiji.Templates/blob/main/LICENSE)

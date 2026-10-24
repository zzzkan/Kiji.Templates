# Kiji.Fluent

A corporate website with [Fluent UI Blazor](https://github.com/microsoft/fluentui-blazor), searchable resources, and responsive light/dark layouts.

Create a site:

```sh
dotnet new kiji-fluent -o MySite --base-url https://example.org/ --site-name "Your Company"
cd MySite
dotnet watch
```

| Parameter            | Description                                   | Default                                                                            |
| -------------------- | --------------------------------------------- | ---------------------------------------------------------------------------------- |
| `--base-url`         | Public URL, including any deployment subpath. | `https://example.com/`                                                             |
| `--site-name`        | Site display name.                            | `Kiji Fluent`                                                                      |
| `--site-description` | Site description.                             | `A corporate website with Fluent UI components and a searchable resource library.` |
| `--site-language`    | Language tag, such as `en` or `ja`.           | `en`                                                                               |
| `--site-author`      | Site author.                                  | `zzzkan`                                                                           |

Edit these values later in `Program.cs`. The language tag sets document metadata;
translate the page text separately.

## Make it yours

- Replace the sample content in `Pages/Home.razor` and `Layout/MainLayout.razor`.
- Edit resource links and categories in `Models/Resource.cs`.
- Adjust page spacing in `wwwroot/css/app.css`. Components use Fluent UI's
  default theme, which follows the system's light/dark preference.
- Edit search and filters in `Pages/Home.razor.js`. Use JavaScript for browser
  interactions; Blazor's `@onclick` and `@bind` do not run on this static site.

## Publish

Set your public `BaseUrl` in `Program.cs`, including any subpath such as
`https://example.org/my-site/`, then run:

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. No .NET server is needed.

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/) and
[Fluent UI Blazor components](https://www.fluentui-blazor.net/) for more ideas.

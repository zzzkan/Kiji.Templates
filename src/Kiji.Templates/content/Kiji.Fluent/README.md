# Kiji.Fluent

A corporate website built with [Fluent UI Blazor](https://github.com/microsoft/fluentui-blazor)
5.0.1 and Kiji. Includes service introductions, a delivery approach, searchable
resources, a project planning checklist, and a technology page. Responsive
layouts and automatic light/dark colors use a restrained navy and blue palette.

```sh
dotnet new kiji-fluent -o MySite --base-url https://example.org/ --site-name "Your Company"
cd MySite
dotnet watch
```

Open <http://localhost:8080>. Changes rebuild the preview; press `Ctrl+C` to stop.
The .NET 10 SDK is required.

| Parameter | Description | Default |
| --- | --- | --- |
| `--base-url` | Public URL, including any deployment subpath. | `https://example.com/` |
| `--site-name` | Site display name. | `Kiji Fluent` |
| `--site-description` | Site description. | `A corporate website with Fluent UI components and a searchable resource library.` |
| `--site-language` | Language tag, such as `en` or `ja`. | `en` |
| `--site-author` | Site author. | `zzzkan` |

These values are written to `SiteInfo` in `Program.cs`. The language tag sets
document metadata; translate the page text separately when changing languages.

## Make it yours

- Edit the links, categories, and descriptions in `Models/Resource.cs`.
  Category buttons are generated from the resource list.
- Change the pages in `Pages/`, the shared layout in `Layout/MainLayout.razor`,
  and the colors and spacing in `wwwroot/css/app.css`.
- Replace the sample service and approach text with your company's content.
  Icons are ordinary emoji text in Razor and `Models/Resource.cs`; there are no
  custom icon images or SVGs. Their appearance follows the visitor's platform.
- `Components/ResourceCard.razor` composes Fluent UI cards and badges. Its
  `.razor.css` file demonstrates CSS isolation alongside the RCL's own styles.
- `Pages/Home.razor.js` handles filtering and the native project checklist dialog.
  The checklist helps visitors prepare a brief; it does not collect or submit data.

## Using a Razor class library

`Program.cs` registers Fluent UI services with `UseServices`. Kiji renders the
Razor components at build time and publishes the NuGet RCL's static web assets
under `_content/`. `Components/PageHead.razor` references the generated scoped
stylesheet through `Assets`; it imports the RCL's scoped CSS. The home page
resolves the RCL module through `Site.ResolveUrl`, so deployment subpaths work
too. `Pages/Home.razor.js` explicitly calls the pinned Fluent UI version's
`beforeStart` initializer to register web components and the system theme.
The technology and not-found pages need no JavaScript.

The published site uses HTML, CSS, and JavaScript. It does not run Blazor:
`@onclick`, `@bind`, and interactive Blazor services do not execute in the
browser. Choose static-friendly components and wire browser interactions with
JavaScript, as this template does. Buttons have explicit roles and search uses
a native HTML input, so neither depends on Blazor's accessibility setup.
The complete resource collection and ordinary
navigation links remain usable with JavaScript disabled.

## Publish

Set your public `BaseUrl` in `Program.cs`, including any subpath such as
`https://example.org/my-site/`, then run:

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. No .NET server is needed.

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/) and
[Fluent UI Blazor components](https://www.fluentui-blazor.net/) for more ideas.

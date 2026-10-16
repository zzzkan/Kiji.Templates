# Travel.Journal

A minimal static website with a single home page, built with Razor and Kiji.
Requires the .NET 10 SDK.

## Preview and edit

```sh
dotnet watch
```

Open <http://localhost:8080>. Edit `Pages/Home.razor` to change the home page.
Set the site name and published URL in `Program.cs`.
Press `Ctrl+C` to stop the preview server.

## Publish

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. The published files do not need
a .NET server.

## Learn more

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/)
for layouts, CSS, and Markdown.

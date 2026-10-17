# Kiji.Interactive

A sample static website recreating the Home, Counter, Weather, and Not Found
pages familiar from the Blazor WebAssembly template. Built with Razor and Kiji,
with browser interactions in JavaScript. Requires the .NET 10 SDK.

## Preview and edit

```sh
dotnet watch
```

Open <http://localhost:8080>. Press `Ctrl+C` to stop the preview server.
Set the site name and published URL in `Program.cs`.

Edit pages in `Pages/` and navigation in `Layout/NavMenu.razor`. Their matching
`.razor.js` files provide the browser interactions. Styles are in
`Layout/*.razor.css` and `wwwroot/css/app.css`; sample forecasts are in
`wwwroot/sample-data/weather.json`.

Navigation loads a new HTML document. The counter and weather data require JavaScript.

## Publish

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. The output contains static
HTML, CSS, JavaScript, and JSON, and needs no .NET server or WebAssembly runtime.

For a subdirectory deployment, set `BaseUrl` in `Program.cs` to the full public
URL, for example `https://example.com/my-site/`, before publishing. Configure your
host to serve directory indexes and use the generated `404.html` for missing URLs.

## Learn more

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/)
for more pages, layouts, CSS, and Markdown.

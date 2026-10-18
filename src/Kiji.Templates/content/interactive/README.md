# Kiji.Interactive

Home, Counter, and Weather follow the .NET 10 Blazor WebAssembly template,
using Bootstrap 5.3.3, Bootstrap Icons, and scoped Razor CSS. Kiji renders the
pages as static HTML; the collocated `.razor.js` files handle the navigation
menu, counter, and weather request in the browser.

Run `dotnet watch` to start a local preview at <http://localhost:8080>.
The site rebuilds when you edit files. Press `Ctrl+C` to stop.

To publish, set `BaseUrl` in `Program.cs` to your site's public URL, then run
`dotnet publish -c Release`. Upload the contents of `dist/` to your static host;
no .NET server is needed.

For more about creating pages and customizing your site, see the
[Kiji documentation](https://kiji-docs.zzzkan.workers.dev/).

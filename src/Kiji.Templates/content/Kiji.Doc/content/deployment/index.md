---
title: Deployment
description: Publish static HTML and deploy it at a domain root or subpath.
order: 30
---

## Set the public URL

Before publishing, set `BaseUrl` in `Program.cs` to the final public address.
Include a trailing slash and any subpath, such as `https://example.com/handbook/`.
Navigation links, canonical URLs, assets, and the sitemap use this address.

## Publish

Run this command from the project directory:

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. The site runs as ordinary
HTML, CSS, and JavaScript and does not require a .NET server or the Blazor
runtime.

## Configure your host

Configure your host to serve each directory URL from its `index.html` file and
to use `404.html` for missing pages. When the site is ready, submit
`sitemap.xml` to search engines.

## Check your documentation

- Follow links from the home page to each article.
- Open the navigation on a narrow screen.
- Check code examples in light and dark mode.
- Confirm that the deployed URL matches `BaseUrl`.

The content and navigation work without JavaScript. Syntax highlighting requires
JavaScript and access to the Prism CDN.

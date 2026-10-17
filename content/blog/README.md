# Kiji.Blog

A small Markdown blog built with Razor and Kiji. Requires the .NET 10 SDK.
Includes a dated post list, About page, RSS feed, and automatic light/dark colors.
Published pages work without JavaScript or external fonts.

## Preview and customize

```sh
dotnet watch
```

Open <http://localhost:8080>. Press `Ctrl+C` to stop the preview.
Set your site's name, description, language, and published URL in `Program.cs`.
Edit the introduction in `Pages/Home.razor`, your biography in `Pages/About.razor`,
navigation and footer in `Layout/MainLayout.razor`, and colors in `wwwroot/css/blog.css`.

## Write a post

Create `contents/my-new-post/index.md`:

```markdown
---
title: My new post
description: A short summary for the page and RSS feed.
date: 2026-03-01T09:00:00Z
---

Write your post here. Use ## for section headings; the title is already shown.
```

The folder name becomes `/posts/my-new-post/`. Use a unique lowercase,
hyphen-separated folder name for each post, with one `index.md` inside it.
Title and date are required. Use an ISO 8601 date with a time-zone offset (or `Z`
for UTC). Posts appear newest first on the home page and in `feed.xml`.
All Markdown files in `contents/` are published, including future-dated posts;
keep unfinished drafts outside that directory. Remove the two sample folders
when you are ready to replace them.

Place images beside a post's `index.md` and reference them as `![Description](photo.jpg)`.
Link to another post with `../other-post/` and to About with `../../about/`.
Document-relative links also work when the site is hosted in a subdirectory.

## Publish

```sh
dotnet publish -c Release
```

Upload the contents of `dist/` to your static host. No .NET server is needed.
For a subdirectory deployment, first set `BaseUrl` in `Program.cs` to the full
public URL, for example `https://example.com/my-blog/`. This also sets canonical
and RSS URLs. Configure your host to serve directory indexes and use `404.html`
for missing pages.

See the [Kiji documentation](https://kiji-docs.zzzkan.workers.dev/docs/getting-started/)
for more about pages, Markdown, and publishing.

---
title: Writing documentation
description: Add Markdown articles, arrange your navigation, and include code examples.
order: 20
---

## Add an article

Create `content/your-topic/index.md`, or use a single file such as
`content/your-topic.md`. Both publish at `docs/your-topic/`.
Use unique names containing lowercase ASCII letters, digits, and hyphens.

```yaml
---
title: Your topic
description: A short explanation of what readers will learn.
order: 40
---
```

Write your content after the front matter. The title becomes the page's main
heading, so start the body with `##` headings.

## Arrange the navigation

The sidebar, mobile menu, and home page discover articles automatically.
Lower `order` values appear first; ties sort by the URL slug.

| Field | Purpose |
| --- | --- |
| `title` | Required page heading and navigation label |
| `description` | Home page summary and search engine description |
| `order` | Navigation position, defaulting to zero |

## Link to other pages

Use relative links such as `[Getting started](../getting-started/)` between
articles. Include the trailing slash. These links also work when the site is
published below a subpath.

## Show code

Add a language to a fenced code block, such as `csharp`, `sh`, or `json`.
Prism loads the matching grammar from its CDN.

> Keep examples focused on one task, and explain what the reader should expect.

## Customize the layout

Edit `Layout/MainLayout.razor` for the header and footer, and `wwwroot/css/app.css`
for shared typography and colors. Component styles live beside their Razor files.

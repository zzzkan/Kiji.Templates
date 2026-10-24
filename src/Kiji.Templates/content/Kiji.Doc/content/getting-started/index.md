---
title: Getting started
description: Set up your documentation site and make it your own.
order: 10
---

Welcome to your documentation. Replace these sample guides with instructions for
your project, product, or team.

## Preview your site

Install the .NET 10 SDK, then run this command from your project directory:

```sh
dotnet watch
```

Open <http://localhost:8080>. Changes to your Markdown, Razor components, and
styles rebuild the preview. Press **Ctrl+C** to stop.

## Make it yours

Edit `Program.cs` to set your site's name, description, language, author, and public
URL. The home page and metadata use these values automatically.

```csharp
app.Info = new SiteInfo
{
    Name = "Project handbook",
    Description = "Everything you need to get started.",
    BaseUrl = new Uri("https://example.com/handbook/"),
    Language = "en",
    Author = "Your team",
};
```

## Next steps

Learn how to [write documentation](../writing/) and [publish your site](../deployment/).

#!/usr/bin/env dotnet
#:property PublishAot=false
#:package CliWrap@3.10.5
#:include TemplatePackage.cs
#:include DotnetCli.cs

using System.Net;
using System.Text.Json;
using Kiji.Templates.Infrastructure;

if (args.Length != 4)
    throw new ArgumentException("Usage: dotnet run --file src/build-demos.cs -- <package-or-directory> <base-url> <work-directory> <output-directory>");

var package = new TemplatePackage(args[0]);
var baseUrl = new Uri(args[1], UriKind.Absolute);
if (baseUrl.Scheme is not ("http" or "https") || baseUrl.Query.Length > 0 || baseUrl.Fragment.Length > 0)
    throw new ArgumentException("Base URL must be HTTP(S) without a query or fragment.");
var work = Path.GetFullPath(args[2]);
var output = Path.GetFullPath(args[3]);
if (Directory.Exists(work) || Directory.Exists(output))
    throw new InvalidOperationException("Use fresh work and output directories.");
Directory.CreateDirectory(output);
var dotnet = new DotnetCli(Directory.GetCurrentDirectory(), work);
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
await dotnet.Run("install", cancellation.Token, "new", "install", package.Path);

var links = new List<string>();
foreach (var template in package.Templates)
{
    var project = Path.Combine(work, template.Kind);
    await dotnet.Run($"{template.Kind}-new", cancellation.Token,
        "new", template.ShortName, "-o", project, "--no-restore");
    var programPath = Path.Combine(project, "Program.cs");
    var program = await File.ReadAllTextAsync(programPath, cancellation.Token);
    const string placeholder = "BaseUrl = new Uri(\"https://example.com/\")";
    if (!program.Contains(placeholder, StringComparison.Ordinal))
        throw new InvalidOperationException($"Missing BaseUrl placeholder in {programPath}.");
    var demoUrl = $"{baseUrl.AbsoluteUri.TrimEnd('/')}/{template.Kind}/";
    await File.WriteAllTextAsync(programPath,
        program.Replace(placeholder, $"BaseUrl = new Uri({JsonSerializer.Serialize(demoUrl)})"), cancellation.Token);
    await dotnet.Run($"{template.Kind}-publish", cancellation.Token,
        "publish", Directory.GetFiles(project, "*.csproj").Single(), "-c", "Release",
        "-o", Path.Combine(output, template.Kind), "--disable-build-servers",
        $"-bl:{Path.Combine(dotnet.Logs, template.Kind)}.binlog");
    if (!File.Exists(Path.Combine(output, template.Kind, "index.html")))
        throw new InvalidOperationException($"No index.html published for {template.Kind}.");
    links.Add($"    <li><a href=\"./{Uri.EscapeDataString(template.Kind)}/\">{WebUtility.HtmlEncode(template.Kind)}</a></li>");
}

await File.WriteAllTextAsync(Path.Combine(output, "index.html"), $$"""
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1">
      <title>Kiji template demos</title>
    </head>
    <body>
      <h1>Kiji template demos</h1>
      <ul>
    {{string.Join('\n', links)}}
      </ul>
    </body>
    </html>
    """, cancellation.Token);

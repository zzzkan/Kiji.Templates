param(
    [Parameter(Mandatory)]
    [string] $Package,
    [Parameter(Mandatory)]
    [string] $RunDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Package = (Resolve-Path $Package).Path
$RunDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($RunDirectory)

dotnet tool run dotnet-template-authoring -- validate $Package

$archive = [System.IO.Compression.ZipFile]::OpenRead($Package)
try {
    $paths = @($archive.Entries.FullName)
    if ($paths -match '^content/(?:.*/)?(?:bin|obj|dist|\.kiji|\.vs)/') {
        throw 'The package includes generated or IDE output.'
    }
} finally {
    $archive.Dispose()
}

dotnet new install $Package
New-Item -ItemType Directory -Force "$RunDirectory/logs" | Out-Null

foreach ($kind in @('minimal', 'interactive', 'blog')) {
    $output = "$RunDirectory/$kind"
    if (Test-Path $output) {
        throw "Use a fresh verification directory: $output"
    }

    # Only Interactive's head contains a project-name-dependent stylesheet reference.
    $headSnapshot = if ($kind -eq 'interactive') { @('--include-pattern', '**PageHead.razor') } else { @() }
    # Suffix globs work with both Windows and Unix path separators.
    dotnet tool run dotnet-template-authoring -- verify "kiji-$kind" `
        --template-path $Package `
        --scenario-name named --template-args '-n Travel.Journal --no-restore' `
        --output "$output/generated site" --snapshots-directory tests/snapshots `
        --disable-diff-tool --disable-default-exclude-patterns `
        --include-pattern '**.csproj' --include-pattern '**Program.cs' `
        --include-pattern '**README.md' --include-pattern '**_Imports.razor' @headSnapshot

    $namedSite = "$output/generated site/kiji-$kind"
    if (Test-Path "$namedSite/obj/project.assets.json") {
        throw "${kind}: --no-restore did not skip dependency restore."
    }

    dotnet new "kiji-$kind" -o "$output/MySite"
    if (!(Test-Path "$output/MySite/MySite.csproj") -or
        (Get-Content "$output/MySite/Program.cs" -Raw) -notmatch 'Name = "MySite"' -or
        (Get-Content "$output/MySite/README.md" -Raw) -notmatch '^# MySite\r?\n') {
        throw "${kind}: The output directory name was not used as the project and site name."
    }

    dotnet publish "$output/MySite/MySite.csproj" -c Release --no-restore `
        "-bl:$RunDirectory/logs/$kind-publish.binlog"
    $homeHtml = Get-Content "$output/MySite/dist/index.html" -Raw
    $expected = switch ($kind) {
        'minimal' { 'Hello from Kiji' }
        'interactive' { 'Hello, world!' }
        'blog' { 'A place for small notes' }
    }
    if (!$homeHtml.Contains($expected)) {
        throw "${kind}: Published home page is missing its expected content."
    }
    if ($kind -in @('interactive', 'blog')) {
        $programPath = "$namedSite/Program.cs"
        (Get-Content $programPath -Raw).Replace('https://example.com/', "https://example.com/Kiji.Templates/$kind/") |
            Set-Content $programPath -NoNewline
        dotnet publish "$namedSite/Travel.Journal.csproj" -c Release -o "$output/subpath" `
            "-bl:$RunDirectory/logs/$kind-subpath-publish.binlog"
    }
    if ($kind -eq 'blog') {
        # Exercise the documented authoring workflow, without changing the browser fixtures.
        $probe = "$namedSite/contents/verification-post"
        New-Item -ItemType Directory $probe | Out-Null
        @'
---
title: A newly added post
description: Added during template verification.
date: 2026-03-01T09:00:00Z
---

This **new post** must be published automatically.
'@ | Set-Content "$probe/index.md"
        dotnet publish "$namedSite/Travel.Journal.csproj" -c Release -o "$output/added-post" `
            "-bl:$RunDirectory/logs/blog-added-post.binlog"
        $addedHome = Get-Content "$output/added-post/index.html" -Raw
        $addedPost = Get-Content "$output/added-post/posts/verification-post/index.html" -Raw
        [xml] $feed = Get-Content "$output/added-post/feed.xml" -Raw
        if (!$addedHome.Contains('href="/Kiji.Templates/blog/posts/verification-post/"') -or
            !$addedPost.Contains('<strong>new post</strong>') -or
            $feed.rss.channel.item.Count -ne 3 -or
            $feed.rss.channel.item[0].title -ne 'A newly added post' -or
            $feed.rss.channel.item[0].link -ne 'https://example.com/Kiji.Templates/blog/posts/verification-post/') {
            throw 'Blog: Adding a Markdown post did not update the page, home page, and RSS feed.'
        }
    }
}

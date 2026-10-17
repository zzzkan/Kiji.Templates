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

foreach ($kind in @('minimal', 'starter')) {
    $output = "$RunDirectory/$kind"
    if (Test-Path $output) {
        throw "Use a fresh verification directory: $output"
    }

    # Suffix globs work with both Windows and Unix path separators.
    dotnet tool run dotnet-template-authoring -- verify "kiji-$kind" `
        --template-path $Package `
        --scenario-name named --template-args '-n Travel.Journal --no-restore' `
        --output "$output/generated site" --snapshots-directory tests/snapshots `
        --disable-diff-tool --disable-default-exclude-patterns `
        --include-pattern '**.csproj' --include-pattern '**Program.cs' `
        --include-pattern '**README.md' --include-pattern '**_Imports.razor' `
        --include-pattern '**PageHead.razor'

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
    $expected = if ($kind -eq 'minimal') { 'Hello from Kiji' } else { 'Hello, world!' }
    if (!$homeHtml.Contains($expected)) {
        throw "${kind}: Published home page is missing its expected content."
    }
    if ($kind -eq 'starter') {
        $programPath = "$namedSite/Program.cs"
        (Get-Content $programPath -Raw).Replace('https://example.com/', 'https://example.com/Kiji.Templates/starter/') |
            Set-Content $programPath -NoNewline
        dotnet publish "$namedSite/Travel.Journal.csproj" -c Release -o "$output/subpath" `
            "-bl:$RunDirectory/logs/starter-subpath-publish.binlog"
    }
}

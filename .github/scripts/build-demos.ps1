#requires -Version 7.4
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $PackagePath,
    [Parameter(Mandatory)][uri] $BaseUrl,
    [Parameter(Mandatory)][string] $WorkDirectory,
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$package = [IO.Path]::GetFullPath($PackagePath, $repository)
$work = [IO.Path]::GetFullPath($WorkDirectory, $repository)
$output = [IO.Path]::GetFullPath($OutputDirectory, $repository)
if (!$BaseUrl.IsAbsoluteUri -or $BaseUrl.Scheme -notin @('http', 'https') -or $BaseUrl.Query -or $BaseUrl.Fragment) {
    throw 'Base URL must be HTTP(S) without a query or fragment.'
}
if ((Test-Path -LiteralPath $work) -or (Test-Path -LiteralPath $output)) {
    throw 'Use fresh work and output directories.'
}
if (Test-Path -LiteralPath $package -PathType Container) {
    $packages = @(Get-ChildItem -LiteralPath $package -Filter '*.nupkg' -File)
    if ($packages.Count -ne 1) {
        throw "Expected exactly one .nupkg in $package; found $($packages.Count)."
    }
    $package = $packages[0].FullName
}

$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $templates = @(foreach ($entry in $archive.Entries) {
        if ($entry.FullName -cmatch '^content/([^/]+)/\.template\.config/template\.json$') {
            $kind = $Matches[1]
            $reader = [IO.StreamReader]::new($entry.Open())
            try {
                $definition = $reader.ReadToEnd() | ConvertFrom-Json
                [pscustomobject]@{ Kind = $kind; ShortName = $definition.shortName }
            }
            finally { $reader.Dispose() }
        }
    })
}
finally { $archive.Dispose() }
if ($templates.Count -eq 0) {
    throw "No templates found in $package."
}

$logs = [IO.Directory]::CreateDirectory((Join-Path $work 'logs')).FullName
$null = [IO.Directory]::CreateDirectory($output)

function Invoke-Dotnet([string] $Label, [string[]] $CommandArguments) {
    Write-Host "Running $Label (logs: $logs/$Label.*.log)"
    if ($CommandArguments[0] -eq 'new') {
        $CommandArguments += @('--debug:custom-hive', (Join-Path $work 'templates'))
    }
    Push-Location $repository
    try {
        & dotnet @CommandArguments > (Join-Path $logs "$Label.stdout.log") 2> (Join-Path $logs "$Label.stderr.log")
    }
    finally { Pop-Location }
}

Invoke-Dotnet 'install' @('new', 'install', $package)
$links = foreach ($template in ($templates | Sort-Object Kind)) {
    $project = Join-Path $work $template.Kind
    Invoke-Dotnet "$($template.Kind)-new" @('new', $template.ShortName, '-o', $project, '--no-restore')
    $programPath = Join-Path $project 'Program.cs'
    $program = [IO.File]::ReadAllText($programPath)
    $placeholder = 'BaseUrl = new Uri("https://example.com/")'
    if (!$program.Contains($placeholder)) {
        throw "Missing BaseUrl placeholder in $programPath."
    }
    $demoUrl = "$($BaseUrl.AbsoluteUri.TrimEnd('/'))/$($template.Kind)/"
    $urlLiteral = ConvertTo-Json -InputObject $demoUrl -Compress
    [IO.File]::WriteAllText($programPath, $program.Replace($placeholder, "BaseUrl = new Uri($urlLiteral)"))
    $projects = @(Get-ChildItem -LiteralPath $project -Filter '*.csproj' -File)
    if ($projects.Count -ne 1) {
        throw "Expected exactly one project in $project; found $($projects.Count)."
    }
    $siteOutput = Join-Path $output $template.Kind
    Invoke-Dotnet "$($template.Kind)-publish" @(
        'publish', $projects[0].FullName, '-c', 'Release', '-o', $siteOutput,
        '-p:UseCurrentRuntimeIdentifier=true', '-p:SelfContained=false',
        "-p:CustomAfterMicrosoftCommonTargets=$(Join-Path $PSScriptRoot 'verification.targets')",
        "-bl:$(Join-Path $logs "$($template.Kind).binlog")"
    )
    if (!(Test-Path -LiteralPath (Join-Path $siteOutput 'index.html') -PathType Leaf)) {
        throw "No index.html published for $($template.Kind)."
    }
    '    <li><a href="./{0}/">{1}</a></li>' -f [Uri]::EscapeDataString($template.Kind), [Net.WebUtility]::HtmlEncode($template.Kind)
}

[IO.File]::WriteAllText((Join-Path $output 'index.html'), @"
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
$($links -join "`n")
  </ul>
</body>
</html>
"@)

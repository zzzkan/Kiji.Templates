#requires -Version 7.4
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $PackagePath,
    [Parameter(Mandatory)][string] $BaseUrl,
    [Parameter(Mandatory)][string] $WorkDirectory,
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

$package = (Get-Item -LiteralPath $PackagePath).FullName
$work = [IO.Path]::GetFullPath($WorkDirectory, $PWD.Path)
$output = [IO.Path]::GetFullPath($OutputDirectory, $PWD.Path)
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
        if ($entry.FullName -cmatch '^content/[^/]+/\.template\.config/template\.json$') {
            $reader = [IO.StreamReader]::new($entry.Open())
            try {
                $reader.ReadToEnd() | ConvertFrom-Json
            }
            finally { $reader.Dispose() }
        }
    })
}
finally { $archive.Dispose() }
if ($templates.Count -eq 0) {
    throw "No templates found in $package."
}

$null = [IO.Directory]::CreateDirectory($output)
$hive = Join-Path $work 'templates'

dotnet new install $package --debug:custom-hive $hive
$templates = $templates | Sort-Object shortName
foreach ($template in $templates) {
    $name = $template.shortName
    $project = Join-Path $work $name
    dotnet new $name --name $template.sourceName -o $project `
        --base-url "$BaseUrl/$name/" --no-restore --debug:custom-hive $hive
    $siteOutput = Join-Path $output $name
    dotnet publish (Join-Path $project "$($template.sourceName).csproj") -c Release -o $siteOutput `
        -p:UseCurrentRuntimeIdentifier=true -p:SelfContained=false `
        "-p:CustomAfterMicrosoftCommonTargets=$(Join-Path $PSScriptRoot 'verification.targets')"
    if (!(Test-Path -LiteralPath (Join-Path $siteOutput 'index.html') -PathType Leaf)) {
        throw "No index.html published for $name."
    }
}

$links = foreach ($template in $templates) {
    '    <li><a href="./{0}/">{0}</a></li>' -f $template.shortName
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

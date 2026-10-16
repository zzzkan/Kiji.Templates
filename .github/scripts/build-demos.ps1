param(
    [Parameter(Mandatory)]
    [uri] $BaseUrl,
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z][a-z0-9-]*$')]
    [string[]] $Templates,
    [string] $WorkDirectory = 'artifacts/verification/demos',
    [string] $OutputDirectory = 'artifacts/verification/pages'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if (!$BaseUrl.IsAbsoluteUri -or $BaseUrl.Scheme -notin @('http', 'https') -or
    $BaseUrl.Query -or $BaseUrl.Fragment) {
    throw 'BaseUrl must be an absolute HTTP(S) URL without a query or fragment.'
}
foreach ($directory in @($WorkDirectory, $OutputDirectory)) {
    if (Test-Path $directory) {
        throw "Use a fresh directory for demo builds: $directory"
    }
}
$WorkDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($WorkDirectory)
$OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
New-Item -ItemType Directory -Path "$WorkDirectory/logs", $OutputDirectory | Out-Null

$links = foreach ($kind in $Templates) {
    $projectDirectory = Join-Path $WorkDirectory $kind
    dotnet new "kiji-$kind" -o $projectDirectory --no-restore | Out-Host

    # Set the deployment URL only in the generated demo, keeping templates portable.
    $programPath = Join-Path $projectDirectory 'Program.cs'
    $program = Get-Content $programPath -Raw
    $placeholder = 'BaseUrl = new Uri("https://example.com/")'
    if (!$program.Contains($placeholder)) {
        throw "Template '$kind' must declare $placeholder so its demo URL can be configured."
    }
    $demoUrl = "$($BaseUrl.AbsoluteUri.TrimEnd('/'))/$kind/"
    $program.Replace($placeholder, "BaseUrl = new Uri(`"$demoUrl`")") |
        Set-Content $programPath -NoNewline

    $projects = @(Get-ChildItem "$projectDirectory/*.csproj")
    if ($projects.Count -ne 1) {
        throw "Expected one project for '$kind', found $($projects.Count)."
    }
    dotnet publish $projects[0].FullName -c Release -o "$OutputDirectory/$kind" `
        "-bl:$WorkDirectory/logs/$kind.binlog" | Out-Host
    if (!(Test-Path "$OutputDirectory/$kind/index.html")) {
        throw "Demo '$kind' did not publish an index.html."
    }
    "    <li><a href=`"./$kind/`">$kind</a></li>"
}

@"
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
"@ | Set-Content "$OutputDirectory/index.html"

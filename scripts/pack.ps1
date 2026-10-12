#requires -Version 7.3
param(
    [string] $OutputDirectory = "$PSScriptRoot/../artifacts/packages",
    [string] $ReleaseTag
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$project = "$PSScriptRoot/../Kiji.Templates.csproj"
[xml] $metadata = Get-Content -LiteralPath $project -Raw
$version = $metadata.Project.PropertyGroup.PackageVersion
if ($ReleaseTag -and $ReleaseTag -cne "v$version") {
    throw "Release tag '$ReleaseTag' must match PackageVersion: v$version"
}

$output = [IO.Path]::GetFullPath($OutputDirectory, $PWD.Path)
if ((Test-Path -LiteralPath $output) -and (Get-ChildItem -LiteralPath $output -Force)) {
    throw "Package output must be empty: $output. Choose a new directory or remove the old output."
}
dotnet pack $project -c Release -o $output

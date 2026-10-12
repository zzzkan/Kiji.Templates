#requires -Version 7.3
param([string] $PackagePath)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

$repository = [IO.Path]::GetFullPath("$PSScriptRoot/..")
$root = Join-Path $repository "artifacts/verification/$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $root -Force | Out-Null
Write-Host "Verification artifacts: $root"

function Assert-That([bool] $Condition, [string] $Message) {
    if (!$Condition) { throw $Message }
}

function Start-Dotnet([string] $Directory, [string[]] $Arguments) {
    $start = [Diagnostics.ProcessStartInfo]::new((Get-Command dotnet).Source)
    $start.WorkingDirectory = $Directory
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    # Isolate template installation and package restore from the developer's state.
    $start.Environment['DOTNET_CLI_HOME'] = "$root/cli"
    $start.Environment['NUGET_PACKAGES'] = "$root/packages"
    $start.Environment['DOTNET_CLI_UI_LANGUAGE'] = 'en'
    $start.Environment['DOTNET_NOLOGO'] = '1'
    $start.Environment['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
    $start.Environment['DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER'] = '1'
    $start.Environment['DOTNET_WATCH_SUPPRESS_BROWSER_REFRESH'] = '1'
    foreach ($variable in @('KIJI_OUTPUT', 'KIJI_FORCE', 'KIJI_VERBOSE', 'MSBuildSDKsPath')) {
        [void] $start.Environment.Remove($variable)
    }
    $process = [Diagnostics.Process]::Start($start)
    return @{
        Process = $process
        Output = $process.StandardOutput.ReadToEndAsync()
        Errors = $process.StandardError.ReadToEndAsync()
        Log = Join-Path $root "$([IO.Path]::GetFileName($Directory))-$($Arguments[0])-$([Guid]::NewGuid().ToString('N')).log"
    }
}

function Complete-Dotnet($Run) {
    if (!$Run.Process.HasExited) { $Run.Process.Kill($true) }
    $Run.Process.WaitForExit()
    $log = $Run.Output.GetAwaiter().GetResult() + $Run.Errors.GetAwaiter().GetResult()
    [IO.File]::WriteAllText($Run.Log, $log)
    $Run.Process.Dispose()
    return $log
}

function Invoke-Dotnet([string] $Directory, [string[]] $Arguments) {
    Write-Host "dotnet $($Arguments -join ' ')"
    $run = Start-Dotnet $Directory $Arguments
    try {
        $finished = $run.Process.WaitForExit(300000)
        $succeeded = $finished -and $run.Process.ExitCode -eq 0
    } finally {
        $log = Complete-Dotnet $run
    }
    Assert-That $succeeded "dotnet failed or timed out. See $($run.Log)`n$log"
}

function Read-Entry($Archive, [string] $Name) {
    $entry = $Archive.GetEntry($Name)
    Assert-That ($null -ne $entry) "Missing package entry: $Name"
    $reader = [IO.StreamReader]::new($entry.Open())
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

function Assert-Watch([string] $Site, [string] $ExpectedHtml) {
    # Bind an ephemeral port; use it only for this child process.
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    $run = Start-Dotnet $Site @('watch', '--non-interactive', '--', '--urls', "http://127.0.0.1:$port")
    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.UseProxy = $false
    $client = [Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(3)
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds(120)
        do {
            Assert-That (!$run.Process.HasExited) "dotnet watch exited. See $($run.Log)"
            try {
                $html = $client.GetStringAsync("http://127.0.0.1:$port/").GetAwaiter().GetResult()
                if ($html.Contains($ExpectedHtml)) { return }
            } catch [Net.Http.HttpRequestException] {
                # The server is still building/starting.
            } catch [Threading.Tasks.TaskCanceledException] {
                # Retry until the bounded startup deadline.
            }
            Start-Sleep -Milliseconds 500
        } while ([DateTime]::UtcNow -lt $deadline)
        throw "dotnet watch did not serve the expected home page. See $($run.Log)"
    } finally {
        $client.Dispose()
        $null = Complete-Dotnet $run
    }
}

# Block parent build settings so generated projects must stand on their own.
[IO.File]::WriteAllText("$root/Directory.Build.props", '<Project />')
[IO.File]::WriteAllText("$root/Directory.Build.targets", '<Project />')
[IO.File]::WriteAllText("$root/Directory.Packages.props", '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>')
Copy-Item -LiteralPath "$repository/NuGet.Config", "$repository/global.json" -Destination $root

if (!$PackagePath) {
    & "$PSScriptRoot/pack.ps1" -OutputDirectory "$root/packed"
    $packages = @(Get-ChildItem -LiteralPath "$root/packed" -Filter '*.nupkg')
    Assert-That ($packages.Count -eq 1) 'Expected exactly one template package.'
    $PackagePath = $packages[0].FullName
}
$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
$hash = (Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash
[IO.File]::WriteAllText("$root/package.log", "$PackagePath`nSHA256: $hash`n")

$archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
try {
    [xml] $manifest = Read-Entry $archive 'Kiji.Templates.nuspec'
    [xml] $project = Get-Content -LiteralPath "$repository/Kiji.Templates.csproj" -Raw
    Assert-That ($manifest.package.metadata.id -ceq 'Kiji.Templates') 'Unexpected package ID.'
    Assert-That ($manifest.package.metadata.version -ceq $project.Project.PropertyGroup.PackageVersion) 'Unexpected package version.'
    Assert-That ($manifest.package.metadata.packageTypes.packageType.name -ceq 'Template') 'PackageType must be Template.'
    Assert-That (@($manifest.SelectNodes("//*[local-name()='dependency']")).Count -eq 0) 'Template package must not carry runtime dependencies.'
    Assert-That (@($archive.Entries | Where-Object FullName -Match '(^lib/|/(bin|obj|dist|\.kiji|\.vs)/|\.dll$)').Count -eq 0) 'Build output leaked into the package.'

    $cases = @(Get-Content -LiteralPath "$PSScriptRoot/test-cases.json" -Raw | ConvertFrom-Json)
    $configs = @($archive.Entries | Where-Object FullName -Like 'content/*/.template.config/template.json')
    $expectedConfigs = @($cases | ForEach-Object { "content/$($_.directory)/.template.config/template.json" } | Sort-Object)
    Assert-That ((($configs.FullName | Sort-Object) -join "`n") -ceq ($expectedConfigs -join "`n")) 'Every packaged template needs exactly one verification case.'
    Invoke-Dotnet $root @('new', 'install', $PackagePath)
    foreach ($case in $cases) {
        $prefix = "content/$($case.directory)/"
        $config = Read-Entry $archive "${prefix}.template.config/template.json" | ConvertFrom-Json
        Assert-That ($config.shortName -ceq $case.shortName) "Wrong shortName for $($case.directory)."
        [xml] $sourceProject = Read-Entry $archive "$prefix$($config.sourceName).csproj"
        $reference = @($sourceProject.Project.ItemGroup.PackageReference | Where-Object Include -EQ 'Kiji')
        Assert-That ($reference.Count -eq 1 -and $reference[0].Version -match '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') 'Use a concrete, published Kiji version.'
        [xml] $checkedInProject = Get-Content -LiteralPath "$repository/$prefix$($config.sourceName).csproj" -Raw
        $checkedInVersion = ($checkedInProject.Project.ItemGroup.PackageReference | Where-Object Include -EQ 'Kiji').Version
        Assert-That ($reference[0].Version -ceq $checkedInVersion) 'Packaged Kiji reference differs from the template source.'

        foreach ($name in @('MySite', 'Travel.Journal')) {
            $parent = Join-Path $root "$($case.directory)/$name"
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
            $site = Join-Path $parent $(if ($name -eq 'MySite') { $name } else { 'site with spaces' })
            $arguments = @('new', $case.shortName, '-o', $site)
            if ($name -ne 'MySite') { $arguments += @('-n', $name) }
            Invoke-Dotnet $parent $arguments

            $expected = @($archive.Entries | Where-Object {
                $_.FullName.StartsWith($prefix) -and !$_.FullName.StartsWith("${prefix}.template.config/") -and $_.Name
            } | ForEach-Object { $_.FullName.Substring($prefix.Length).Replace($config.sourceName, $name) } | Sort-Object)
            $actual = @(Get-ChildItem -LiteralPath $site -Recurse -File -Force | ForEach-Object {
                [IO.Path]::GetRelativePath($site, $_.FullName).Replace('\', '/')
            } | Sort-Object)
            Assert-That (($expected -join "`n") -ceq ($actual -join "`n")) "Generated file list differs: $site"
            Assert-That (Test-Path -LiteralPath "$site/.gitignore") 'Missing generated .gitignore.'
            Assert-That ([IO.File]::ReadAllText("$site/Program.cs").Contains("Name = `"$name`"")) 'Site name was not replaced.'
            [xml] $generated = Get-Content -LiteralPath "$site/$name.csproj" -Raw
            $generatedVersion = ($generated.Project.ItemGroup.PackageReference | Where-Object Include -EQ 'Kiji').Version
            Assert-That ($generatedVersion -ceq $reference[0].Version) 'Generated Kiji reference differs from the package.'

            Invoke-Dotnet $site @('publish', '-c', 'Release')
            Assert-That ([IO.File]::ReadAllText("$site/dist/index.html").Contains($case.expectedHtml)) 'Published home page is missing.'
            Assert-That (@(Get-ChildItem -LiteralPath "$site/dist" -Recurse -Filter '*.dll').Count -eq 0) 'Site output must not include assemblies.'
            $assets = Get-Content -LiteralPath "$site/obj/project.assets.json" -Raw | ConvertFrom-Json -AsHashtable
            Assert-That ($assets.libraries.ContainsKey("Kiji/$generatedVersion")) 'Restore did not resolve the specified Kiji version.'
            Assert-Watch $site $case.expectedHtml
            Invoke-Dotnet $site @('clean', '-c', 'Release')
            Assert-That (!(Test-Path -LiteralPath "$site/.kiji")) 'Clean did not remove .kiji.'
        }
    }
    Invoke-Dotnet $root @('new', 'uninstall', 'Kiji.Templates')
} finally {
    $archive.Dispose()
}
Assert-That ((Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash -ceq $hash) 'Verification modified the package.'
Write-Host "Verified: $PackagePath`nSHA256: $hash"

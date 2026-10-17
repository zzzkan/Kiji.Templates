---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose template snapshot, packaging, or generated-site failures in this repository.
---

# Verify Kiji.Templates

Use the SDK in `global.json` and PowerShell 7.3+. After template or packaging
changes, run the checks below; `dotnet pack` alone does not validate the template.

In a fresh PowerShell session at the repository root, prepare isolated CLI/NuGet
state and a new output directory:

```pwsh
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$run = Join-Path $PWD "artifacts/verification/$([Guid]::NewGuid().ToString('N'))"
$env:DOTNET_CLI_HOME = "$run/cli"
$env:NUGET_PACKAGES = "$run/packages"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = '0'
dotnet tool restore
```

Pack the template, or skip this block and set `$package` to the absolute path of an
existing `.nupkg` to verify it without repacking:

```pwsh
dotnet pack Kiji.Templates.csproj -c Release -o "$run/packed"
$package = (Get-Item "$run/packed/*.nupkg").FullName
```

Run the checks shared by [CI](../../../.github/workflows/ci.yml) and
[releases](../../../.github/workflows/release.yml):

```pwsh
./.github/scripts/verify-templates.ps1 -Package $package -RunDirectory "$run/sites"
```

The script validates package definitions and exclusions, checks naming and restore,
and publishes both templates. It reuses the explicitly named snapshot project for
Starter's `/Kiji.Templates/starter/` deployment; directory-based naming and automatic
restore are checked separately. Use fresh output directories.

Keep generated-file snapshots focused on name substitution (project files,
`Program.cs`, README, Razor imports, and scoped stylesheet references). Do not
duplicate unchanged page, CSS, JavaScript, or sample-data files.
Review `tests/snapshots/*.received/` against `.verified`; never bulk-accept differences
or copy build output. Publish logs are in `$run/sites/logs/`; CI uploads failure
diagnostics as `verification-<os>`.

## GitHub Pages demos

For demo changes, run the builder used by [Pages](../../../.github/workflows/pages.yml)
with the installed package and isolated environment:

```pwsh
./.github/scripts/build-demos.ps1 `
    -BaseUrl 'https://zzzkan.github.io/Kiji.Templates/' -Templates @('minimal', 'starter') `
    -WorkDirectory "$run/demos" -OutputDirectory "$run/pages"
if ((Get-Content "$run/pages/minimal/index.html" -Raw) -notmatch 'Hello from Kiji' -or
    (Get-Content "$run/pages/starter/index.html" -Raw) -notmatch 'Hello, world!' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./minimal/"' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./starter/"' -or
    (Get-Content "$run/demos/minimal/Program.cs" -Raw) -notmatch
        'https://zzzkan.github.io/Kiji.Templates/minimal/') {
    throw 'Demo content, index link, or deployment URL is incorrect.'
}
```

Use fresh work and output directories. Inspect links and assets under each demo's
deployment path. Publish logs are in `$run/demos/logs/` (`demo-build-logs` in CI).

## Starter browser behavior

After running `verify-templates.ps1`, run the same Chromium checks as CI:

```pwsh
$env:PLAYWRIGHT_BROWSERS_PATH = "$run/browsers"
$env:KIJI_VERIFICATION_DIRECTORY = $run
dotnet build tests/Kiji.Templates.BrowserTests -c Release
pwsh ./tests/Kiji.Templates.BrowserTests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test tests/Kiji.Templates.BrowserTests -c Release --no-build --logger trx --results-directory "$run/logs"
```

The xUnit checks cover accessible content, interactions, and asset/JavaScript errors
at both URL paths. Review inline ARIA expectations in `StarterTests.cs` against the
intended content; never replace them blindly. ARIA snapshots do not check styling:
inspect desktop/mobile screenshots after layout changes. Actual ARIA snapshots and
screenshots are in `$run/logs/root/` and `$run/logs/subpath/`.
The test disposes its static server and browsers, including on failure.

Extend the shared script, snapshots, browser checks, and demo kind lists when adding
template behavior. For changed directory-install exclusions, verify directory
installation too. For preview or clean changes, run `dotnet watch` or `dotnet clean`
in a generated site and check the result. Stop every preview process, including on failure.

Report checks completed and failures. Remove disposable caches and generated sites
when no longer needed, preserving useful failure evidence.

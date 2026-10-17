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
and publishes all three templates. It reuses the explicitly named snapshot projects
for Interactive's `/Kiji.Templates/interactive/` and Blog's `/Kiji.Templates/blog/`
deployments; directory-based naming and automatic restore are checked separately.
Use fresh output directories.

For Blog, it also adds a Markdown post to the generated project and republishes to
a separate directory, checking the new route, rendered Markdown, home link, and RSS
ordering. The browser fixtures retain the original two sample posts.

Keep generated-file snapshots focused on name substitution (project files,
`Program.cs`, README, Razor imports, and scoped stylesheet references). Do not
duplicate unchanged page, CSS, JavaScript, or sample-data files.
Only Interactive's `PageHead.razor` needs a snapshot: its scoped stylesheet name
contains the project name. Blog's head is covered by browser checks.
Review `tests/snapshots/*.received/` against `.verified`; never bulk-accept differences
or copy build output. Publish logs are in `$run/sites/logs/`; CI uploads failure
diagnostics as `verification-<os>`.

## GitHub Pages demos

For demo changes, run the builder used by [Pages](../../../.github/workflows/pages.yml)
with the installed package and isolated environment:

```pwsh
./.github/scripts/build-demos.ps1 `
    -BaseUrl 'https://zzzkan.github.io/Kiji.Templates/' -Templates @('minimal', 'interactive', 'blog') `
    -WorkDirectory "$run/demos" -OutputDirectory "$run/pages"
if ((Get-Content "$run/pages/minimal/index.html" -Raw) -notmatch 'Hello from Kiji' -or
    (Get-Content "$run/pages/interactive/index.html" -Raw) -notmatch 'Hello, world!' -or
    (Get-Content "$run/pages/blog/index.html" -Raw) -notmatch 'A place for small notes' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./minimal/"' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./interactive/"' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./blog/"' -or
    (Get-Content "$run/pages/blog/feed.xml" -Raw) -notmatch
        'https://zzzkan.github.io/Kiji.Templates/blog/posts/keeping-a-notebook/' -or
    (Get-Content "$run/demos/minimal/Program.cs" -Raw) -notmatch
        'https://zzzkan.github.io/Kiji.Templates/minimal/') {
    throw 'Demo content, index link, or deployment URL is incorrect.'
}
```

Use fresh work and output directories. Inspect links and assets under each demo's
deployment path. Publish logs are in `$run/demos/logs/` (`demo-build-logs` in CI).

## Browser behavior

After running `verify-templates.ps1`, run the same Chromium checks as CI:

```pwsh
$env:PLAYWRIGHT_BROWSERS_PATH = "$run/browsers"
$env:KIJI_VERIFICATION_DIRECTORY = $run
dotnet build tests/Kiji.Templates.BrowserTests -c Release
pwsh ./tests/Kiji.Templates.BrowserTests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test tests/Kiji.Templates.BrowserTests -c Release --no-build --logger trx --results-directory "$run/logs"
```

The xUnit checks cover accessible content, interactions, and asset/JavaScript errors
at both URL paths. Blog additionally checks dated ordering, Markdown links, metadata,
RSS, keyboard navigation, mobile overflow, and light/dark colors with JavaScript disabled.
Review inline ARIA expectations in `InteractiveTests.cs` and `BlogTests.cs` against the
intended content; never replace them blindly. ARIA snapshots do not check styling:
inspect desktop/mobile screenshots after layout changes. ARIA snapshots and
screenshots are in `$run/logs/<kind>/root/` and `$run/logs/<kind>/subpath/`.
For Blog, inspect the home and post views on desktop/mobile and the dark post view
when its styles change. `PublishedSiteTest` owns the shared static server, request
error checks, and failure screenshots. The server and browsers are disposed,
including on failure.

Extend the shared script, snapshots, browser checks, and demo kind lists when adding
template behavior. For changed directory-install exclusions, verify directory
installation too. For preview or clean changes, run `dotnet watch` or `dotnet clean`
in a generated site and check the result. Stop every preview process, including on failure.

Report checks completed and failures. Remove disposable caches and generated sites
when no longer needed, preserving useful failure evidence.

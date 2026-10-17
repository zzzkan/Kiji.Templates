---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose packaging, generation, publishing, or browser failures.
---

# Verify Kiji.Templates

Use the SDK in `global.json`. After template or packaging changes, run the package
and generated-site tests; `dotnet pack` alone does not cover site behavior.

Prepare isolated state at the repository root (PowerShell example):

```pwsh
$env:KIJI_VERIFICATION_DIRECTORY = "$PWD/artifacts/verification"
$env:DOTNET_CLI_HOME = "$env:KIJI_VERIFICATION_DIRECTORY/cli"
$env:NUGET_PACKAGES = "$env:KIJI_VERIFICATION_DIRECTORY/packages"
$env:PLAYWRIGHT_BROWSERS_PATH = "$env:KIJI_VERIFICATION_DIRECTORY/browsers"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = '0'
```

Pack into a fresh directory, or skip packing and set `KIJI_TEMPLATE_PACKAGE` to
the absolute path of an existing `.nupkg`. A directory containing exactly one
`.nupkg` is also accepted. Never repack an existing artifact to verify it.

```pwsh
$env:KIJI_TEMPLATE_PACKAGE = "$env:KIJI_VERIFICATION_DIRECTORY/packed-$([Guid]::NewGuid().ToString('N'))"
dotnet pack src/Kiji.Templates/Kiji.Templates.csproj -c Release -o $env:KIJI_TEMPLATE_PACKAGE
```

Run the same checks as [CI](../../../.github/workflows/ci.yml):

```pwsh
dotnet build src/Kiji.Templates.Tests -c Release
pwsh ./src/Kiji.Templates.Tests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test src/Kiji.Templates.Tests -c Release --no-build --logger trx --results-directory "$env:KIJI_VERIFICATION_DIRECTORY/logs"
```

Run each command only after the preceding command succeeds. No additional
`dotnet tool` installation is required. `Microsoft.TemplateEngine.Authoring.Tasks`
validates source definitions during build; `PackageTests` invokes its MSBuild
validation target against the supplied package without building or packing it.

Tests discover templates from the package, compare the list with
`src/Kiji.Templates/content/`, and check package exclusions. Every template gets
two generation/publish/browser
cases: directory-based naming with automatic restore at `/`, and explicit naming
with `--no-restore` at `/Kiji.Templates/<kind>/`. Naming assertions replace
generated-file snapshots. New templates automatically join these common cases.

Interactive and Blog retain their specific browser checks. Blog also adds a
Markdown article to an independent generated project and checks the page, home
link, and RSS. Tests lazily prepare their sites, so filtered tests work without
running other tests first.

Each test run uses a fresh `tests-<id>/` under `KIJI_VERIFICATION_DIRECTORY`.
Command output, publish/validation binlogs, ARIA output, and browser screenshots
are under that run's `logs/`; TRX files are in the requested results directory.
CI uploads these logs on failure. Read stdout/stderr and the binlog for the failed
command before changing tests or template content.

Review inline ARIA expectations in `InteractiveTests.cs` and `BlogTests.cs`
deliberately; never replace them blindly. ARIA assertions do not check styling.
After layout changes, inspect the desktop/mobile screenshots, including Blog's
home, post, and dark post views. Preview servers and browsers are disposed on
failure as well as success.

## GitHub Pages demos

Use the builder shared with [Pages](../../../.github/workflows/pages.yml):

```pwsh
dotnet run --file src/build-demos.cs -- $env:KIJI_TEMPLATE_PACKAGE 'https://zzzkan.github.io/Kiji.Templates/' "$env:KIJI_VERIFICATION_DIRECTORY/demos" "$env:KIJI_VERIFICATION_DIRECTORY/pages"
```

Use fresh work and output directories. The builder discovers all templates in
the package, installs it in isolated CLI state, sets each generated site's BaseUrl,
publishes, and writes the demo index. Inspect links and assets at each deployment
path. Command output and binlogs are in the work directory's `logs/`.

Add specific tests only for new template behavior. For changes to directory-install
exclusions, also verify directory installation. For preview or clean changes, run
`dotnet watch` or `dotnet clean` in a generated project and check the result.
Stop every preview process, including on failure.

Report completed checks and limitations. Preserve useful diagnostics when
removing disposable caches or generated sites. Releases must test the same package
on Windows and Linux, then publish that file without repacking.

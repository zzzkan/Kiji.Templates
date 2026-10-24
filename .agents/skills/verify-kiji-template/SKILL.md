---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose packaging, generation, publishing, or browser failures.
---

# Verify Kiji.Templates

## Verification

After template or packaging changes, run these [CI](../../../.github/workflows/ci.yml)
commands in order, stopping on failure. For an existing package, skip packing.

```pwsh
dotnet pack src/Kiji.Templates/Kiji.Templates.csproj -c Release
dotnet build src/Kiji.Templates.Tests -c Release
pwsh ./src/Kiji.Templates.Tests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test --project src/Kiji.Templates.Tests -c Release --no-build --report-xunit-trx
```

CI runs only on Linux using the Chromium command above. To run browser tests
locally on a machine with Microsoft Edge installed, skip the browser install
and run:

```pwsh
dotnet test --project src/Kiji.Templates.Tests -c Release --no-build --report-xunit-trx --environment KIJI_TEST_BROWSER_CHANNEL=msedge
```

`KIJI_TEST_BROWSER_CHANNEL` selects the Playwright launch channel for browser tests;
leaving it unset retains Playwright's default Chromium headless shell.
For Edge, browser tests also pass an explicit context locale as a launch language
so date formatting does not fall back to the machine's regional settings.

Pack and test with the same configuration. Tests read
`src/Kiji.Templates/bin/<Configuration>`; override with
`--environment "KIJI_TEMPLATE_PACKAGE=<nupkg-or-directory>"` on `dotnet test`.
Package directories must contain exactly one `.nupkg`; relative paths resolve
from the repository root. Tests never repack. For package-only checks, append
`--filter-class '*PackageTests'`; filtered tests prepare their own prerequisites.

[Tests](../../../src/Kiji.Templates.Tests/) cover package contents, generation and
publishing at root and subpaths, Interactive/Blog/Doc/Fluent browser behavior, and Blog/Doc authoring.
All templates select the current runtime with `UseCurrentRuntimeIdentifier`.
Minimal's root case keeps the ordinary publish path. Other cases and demos import
`.github/scripts/verification.targets` through command arguments to omit SkiaSharp's
native PDBs without modifying generated project files or packaged templates.
Root cases publish with `--no-restore` after `dotnet new`'s automatic restore;
subpath cases restore during publish. All builds check that only the host's
SkiaSharp native library is copied; optimized builds also check that native PDBs
are omitted. Keep the ordinary smoke case when changing these options.
Tests and demos isolate registrations with `--debug:custom-hive`; recheck
installation and generation after SDK upgrades.

Tests remove generated `bin` and `obj` even on failure. Diagnostics stay under
`TestResults/`; automatic cleanup keeps five recent inactive runs and reports.

## GitHub Pages demos

Build with the script shared by [Pages](../../../.github/workflows/pages.yml):

```pwsh
pwsh ./.github/scripts/build-demos.ps1 -PackagePath src/Kiji.Templates/bin/Release -BaseUrl 'https://zzzkan.github.io/Kiji.Templates' -WorkDirectory artifacts/demos -OutputDirectory dist
```

Use fresh work/output directories and set `-PackagePath` to the existing artifact
when verifying one. Run from the repository root; relative paths resolve from the
current directory. Pass `-BaseUrl` without a trailing slash. Each demo uses its
template's short name as the directory, such as `kiji-minimal/`.
The builder stops on command failure; only Pages enforces a 15-minute timeout.
The builder removes `bin` and `obj` even on failure. Stop previews before replacing
old demos in `artifacts/demos` and `dist`; do not archive copies in `TestResults/`.

## Diagnostics and targeted checks

- Review inline ARIA expectations in `InteractiveTests.cs` and `BlogTests.cs`
  deliberately. For layout changes, inspect desktop/mobile screenshots, including
  Blog home, post, and dark post views; ARIA does not check styling.
- For Fluent changes, inspect the home page at desktop/mobile widths and in dark
  colors. `FluentTests.cs` checks RCL assets, filtering, keyboard activation, and
  subpath links with and without JavaScript. Keep the site free of Blazor runtime requests.
- For Doc changes, inspect the home page and article at desktop/mobile widths and
  in dark colors. `DocTests.cs` checks ordered navigation, active links, mobile
  keyboard navigation without JavaScript, subpath links, and Prism syntax
  highlighting. `DocAuthoringTests.cs` checks discovery, ordering, sitemap entries,
  and diagnostics for missing titles or duplicate routes.
- Add checks only for new behavior. Verify directory installation after exclusion
  changes; run `dotnet watch` or `dotnet clean` in a generated project for preview
  or clean changes.
- Stop every preview process and dispose browsers even on failure. Preserve useful
  diagnostics during cleanup and report completed checks and limitations.

## Releases

[Release](../../../.github/workflows/release.yml) gates packing on successful main
push CI for the tagged commit, then packs once and uses NuGet trusted publishing
without repeating Linux tests. If blocked, rerun after that CI succeeds.
Pages deploys independently.

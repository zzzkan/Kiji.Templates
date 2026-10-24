---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose packaging, generation, publishing, or browser failures.
---

# Verify Kiji.Templates

## Verification

After template or packaging changes, run these [CI](../../../.github/workflows/ci.yml)
commands in order and stop at the first failure. To verify an existing package, skip packing.

```pwsh
dotnet pack src/Kiji.Templates/Kiji.Templates.csproj -c Release
dotnet build src/Kiji.Templates.Tests -c Release
pwsh ./src/Kiji.Templates.Tests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test --project src/Kiji.Templates.Tests -c Release --no-build --report-xunit-trx
```

CI runs only on Linux and installs Chromium with the command above. To run browser
tests locally on a machine with Microsoft Edge installed, skip the browser install
and run:

```pwsh
dotnet test --project src/Kiji.Templates.Tests -c Release --no-build --report-xunit-trx --environment KIJI_TEST_BROWSER_CHANNEL=msedge
```

`KIJI_TEST_BROWSER_CHANNEL` selects the Playwright launch channel for browser tests.
When it is unset, tests use Playwright's default Chromium headless shell. With Edge,
browser tests also pass the context locale as Edge's launch language, so date
formatting does not fall back to the machine's regional settings.

Pack and test with the same configuration. Tests read the package from
`src/Kiji.Templates/bin/<Configuration>`. To use another package, pass
`--environment "KIJI_TEMPLATE_PACKAGE=<nupkg-or-directory>"` to `dotnet test`.
A package directory must contain exactly one `.nupkg`, and relative paths resolve
from the repository root. Tests never repack. For package-only checks, append
`--filter-class '*PackageTests'`; filtered tests prepare their own prerequisites.

[Tests](../../../src/Kiji.Templates.Tests/) cover package contents, generation and
publishing at the root and under subpaths, browser behavior of the Interactive, Blog,
Doc, and Fluent templates, and Blog and Doc authoring.
All templates select the current runtime with `UseCurrentRuntimeIdentifier`.
Minimal's root case uses the ordinary publish path. Other cases and demos import
`.github/scripts/verification.targets` through command arguments to omit SkiaSharp's
native PDBs without modifying generated project files or packaged templates.
Root cases publish with `--no-restore` after `dotnet new`'s automatic restore, and
subpath cases restore during publish. After every publish, tests check that only
the host's SkiaSharp native library is copied and, for optimized publishes, that
native PDBs are omitted. When changing these options, keep Minimal's root case as
the ordinary smoke test. Tests and demos isolate template registrations with
`--debug:custom-hive`; recheck installation and generation after SDK upgrades.

Tests remove generated `bin` and `obj` directories even on failure. Diagnostics stay
under `TestResults/`, where automatic cleanup keeps the five most recent inactive
test runs and the five most recent reports.

## GitHub Pages demos

Build the demos with the script that [Pages](../../../.github/workflows/pages.yml) also uses:

```pwsh
pwsh ./.github/scripts/build-demos.ps1 -PackagePath src/Kiji.Templates/bin/Release -BaseUrl 'https://zzzkan.github.io/Kiji.Templates' -WorkDirectory artifacts/demos -OutputDirectory dist
```

Use fresh work and output directories. When verifying an existing artifact, set
`-PackagePath` to it. Run the script from the repository root; it resolves relative
paths from the current directory. Pass `-BaseUrl` without a trailing slash. Each
demo goes in a directory named after its template's short name, such as `kiji-minimal/`.
The script stops when a command fails and removes `bin` and `obj` even on failure.
Only the Pages workflow enforces a 15-minute timeout. Stop previews before replacing
old demos in `artifacts/demos` and `dist`, and do not archive copies in `TestResults/`.

## Diagnostics and targeted checks

- Review each change to the inline ARIA expectations in `InteractiveTests.cs` and
  `BlogTests.cs`. ARIA checks do not cover styling, so for layout changes, inspect
  desktop and mobile screenshots, including the Blog home, post, and dark post views.
- For Fluent changes, inspect the home page at desktop and mobile widths and in dark
  mode. `FluentTests.cs` checks RCL assets, filtering, keyboard activation, and
  subpath links with and without JavaScript. Keep the site free of Blazor runtime requests.
- For Doc changes, inspect the home page and an article at desktop and mobile widths
  and in dark mode. `DocTests.cs` checks ordered navigation, active links, mobile
  keyboard navigation without JavaScript, subpath links, and Prism syntax
  highlighting. `DocAuthoringTests.cs` checks discovery, ordering, sitemap entries,
  and diagnostics for missing titles or duplicate routes.
- Add checks only for new behavior. After changing exclusions, verify installation
  from a directory. After changing preview or clean behavior, run `dotnet watch` or
  `dotnet clean` in a generated project.
- Stop every preview process and dispose browsers even on failure. Keep useful
  diagnostics during cleanup, and report which checks completed and any limitations.

## Releases

[Release](../../../.github/workflows/release.yml) packs only after a successful CI
run from a push to `main` for the tagged commit. It packs once and publishes through
NuGet trusted publishing without repeating the Linux tests. If this check blocks a
release, rerun Release after that CI run succeeds. Pages deploys independently.

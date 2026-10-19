---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose packaging, generation, publishing, or browser failures.
---

# Verify Kiji.Templates

Run from the repository root with the SDK in `global.json` and standard caches
and build-server defaults. No shell-wide environment overrides or extra tools
are needed.

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
publishing at root and subpaths, Interactive/Blog browser behavior, and Blog authoring.
Minimal's root case keeps the ordinary publish path. Other cases and demos select
the current runtime and import `.github/scripts/verification.targets` through command
arguments to omit SkiaSharp's native PDBs. These settings never modify generated
project files or packaged templates. Explicit restore uses the same settings before
`--no-restore` publishing; optimized builds check that only the host's SkiaSharp
native library is copied. Keep the ordinary smoke case when changing these options.
Tests and demos isolate registrations with `--debug:custom-hive`; recheck
installation and generation after SDK upgrades.

## GitHub Pages demos

Build with the script shared by [Pages](../../../.github/workflows/pages.yml):

```pwsh
pwsh ./.github/scripts/build-demos.ps1 -PackagePath src/Kiji.Templates/bin/Release -BaseUrl 'https://zzzkan.github.io/Kiji.Templates/' -WorkDirectory obj/demos -OutputDirectory dist
```

Use fresh work/output directories and set `-PackagePath` to the existing artifact
when verifying one. Relative paths resolve from the repository root. Inspect links
and assets at every demo path. Logs and binlogs are in `obj/demos/logs/`.
The builder stops on command failure; only Pages enforces a 15-minute timeout.

## Diagnostics and targeted checks

- Read failed-command stdout/stderr and binlogs before changing code. Test diagnostics
  (including ARIA and screenshots) are in `TestResults/tests-<id>/logs/`; CI uploads
  them on failure. TRX defaults to `TestResults/`; `--results-directory` moves only reports.
- Successful runs remove generated projects' `bin/` and `obj/`, retaining source,
  `dist/`, and diagnostics; failed/cancelled runs retain build output. Test classes
  using `TemplateFixture` must record results during teardown after browser cleanup.
- Review inline ARIA expectations in `InteractiveTests.cs` and `BlogTests.cs`
  deliberately. For layout changes, inspect desktop/mobile screenshots, including
  Blog home, post, and dark post views; ARIA does not check styling.
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

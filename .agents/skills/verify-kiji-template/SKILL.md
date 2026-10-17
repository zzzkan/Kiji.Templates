---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose packaging, generation, publishing, or browser failures.
---

# Verify Kiji.Templates

Use the SDK in `global.json`. After template or packaging changes, run the package
and generated-site tests; `dotnet pack` alone does not cover site behavior.

Run from the repository root. Pack and test with the same configuration; tests
use `src/Kiji.Templates/bin/<Configuration>`, which must contain exactly one
`.nupkg`. To select a file or another directory, use `--environment` below.
Keep the normal SDK, NuGet, Playwright, and build-server defaults; no shell
environment setup or additional `dotnet tool` installation is needed.
Tests and demos isolate template registrations using
[`--debug:custom-hive`](https://github.com/dotnet/templating/wiki/Reserved-Aliases).
Verify installation and generation when upgrading the SDK.

Pack when validating source changes. Skip this command when verifying an existing
artifact; never repack it.

```pwsh
dotnet pack src/Kiji.Templates/Kiji.Templates.csproj -c Release
```

Run the same checks as [CI](../../../.github/workflows/ci.yml):

```pwsh
dotnet build src/Kiji.Templates.Tests -c Release
pwsh ./src/Kiji.Templates.Tests/bin/Release/net10.0/playwright.ps1 install --with-deps --only-shell chromium
dotnet test --project src/Kiji.Templates.Tests -c Release --no-build --report-xunit-trx
```

Run each command only after the preceding command succeeds. For an existing
package or a custom pack output, append
`--environment "KIJI_TEMPLATE_PACKAGE=<nupkg-or-directory>"` to the test command.
The directory must contain exactly one `.nupkg`; relative paths resolve from the
repository root. Missing or invalid package input fails; tests never repack it.
Append `--filter-class '*PackageTests'` for package checks only, or `--help` for
available options. Filtered tests prepare their own prerequisites.

Tests validate package definitions and exclusions, compare templates with
`src/Kiji.Templates/content/`, and verify each template at `/` with directory-based
naming and automatic restore, and at `/Kiji.Templates/<kind>/` with explicit naming
and `--no-restore`. Interactive and Blog add browser checks; Blog also checks
publishing a new Markdown article, its home link, and RSS.

Each test run uses a fresh `tests-<id>/` under the repository's `TestResults/`.
Command output, publish/validation binlogs, ARIA output, and browser screenshots
are under that run's `logs/`. TRX reports go to `TestResults/` by default;
`--results-directory` changes only the report location.
After successful verification, the fixture removes only the generated projects'
`bin/` and `obj/` directories, retaining source, published `dist/`, and diagnostics.
Failed or cancelled runs retain build output for investigation. Every test class
using `TemplateFixture` must record its result during teardown, after any browser
cleanup, so assertion and cleanup failures also preserve build output.
CI uploads these logs on failure. Read stdout/stderr and the binlog for the failed
command before changing tests or template content.

Review inline ARIA expectations in `InteractiveTests.cs` and `BlogTests.cs`
deliberately; never replace them blindly. ARIA assertions do not check styling.
After layout changes, inspect the desktop/mobile screenshots, including Blog's
home, post, and dark post views.

## GitHub Pages demos

Use the PowerShell builder in `.github/scripts/`, shared with
[Pages](../../../.github/workflows/pages.yml):

```pwsh
pwsh ./.github/scripts/build-demos.ps1 -PackagePath src/Kiji.Templates/bin/Release -BaseUrl 'https://zzzkan.github.io/Kiji.Templates/' -WorkDirectory obj/demos -OutputDirectory dist
```

Set `-PackagePath` to the existing package path when verifying one. Relative paths
resolve from the repository root. Use fresh work and output directories.
The builder publishes every packaged template at its demo URL and writes the
index. Inspect links and assets at each deployment path. Command output and
binlogs are in the work directory's `logs/`.
PowerShell stops the builder on a failed `dotnet` command. Pages limits the build
step to 15 minutes; local runs have no automatic timeout and can be interrupted
with Ctrl+C.

Add specific tests only for new template behavior. For changes to directory-install
exclusions, also verify directory installation. For preview or clean changes, run
`dotnet watch` or `dotnet clean` in a generated project and check the result.
Stop every preview process and dispose browsers, including on failure.

Report completed checks and limitations. Preserve useful diagnostics when
removing disposable caches or generated sites.

## Releases

[Release](../../../.github/workflows/release.yml) requires a successful main push
run of `ci.yml` for the exact tagged commit. CI covers Windows and Linux; release
does not repeat those tests. After checking CI, release packs the tagged version
once and publishes it through NuGet trusted publishing.

If CI is pending, failed, or missing, release stops before packing. Rerun Release
after that commit's main CI succeeds. Pages deploys independently.

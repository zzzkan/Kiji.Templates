# Contributing to Kiji.Templates

This guide covers template development, CI, and publishing. For creating a site, see [README.md](README.md).

## Develop and verify

Install the SDK specified in `global.json` and PowerShell 7.3 or later.
This repository works independently of a Kiji source checkout.

1. Edit `content/minimal/`. Its project references a concrete, published Kiji version
   and can also be run directly with `dotnet watch --project content/minimal`.
2. Run the same end-to-end verification as CI:

   ```pwsh
   pwsh -NoProfile -File scripts/verify.ps1
   ```

   This packs, installs, and generates sites with both an inferred name and an explicit
   dotted name in a path containing spaces. It checks package contents, version references,
   `dotnet publish`, HTTP output from `dotnet watch`, and `dotnet clean`. CLI state and
   NuGet packages are isolated under `artifacts/verification/<run>/`; your installed
   templates are unaffected. Logs and generated sites remain there for inspection.
3. For a package to install manually:

   ```pwsh
   pwsh -NoProfile -File scripts/pack.ps1
   dotnet new install ./artifacts/packages/Kiji.Templates.0.1.0.nupkg
   dotnet new kiji -o ../MySite
   # After trying it:
   dotnet new uninstall Kiji.Templates
   ```

   Packaging requires an empty output directory. For another build, pass
   `-OutputDirectory artifacts/packages-next` or remove the previous output.
   `dotnet pack -c Release` is also supported. To verify an existing package without
   repacking, use `pwsh scripts/verify.ps1 -PackagePath <path-to-nupkg>`.

## Continuous integration

`.github/workflows/ci.yml` runs on pull requests to `main`, pushes to `main`, and
manual dispatch. Both Windows and Linux run `scripts/verify.ps1`, which checks the
packaged template through the same commands a site author uses:

1. Pack and install the template NuGet package.
2. Generate projects with both inferred and explicit names, including a path with spaces.
3. Restore the declared Kiji version and publish a static home page.
4. Start `dotnet watch` and request the home page over HTTP.
5. Clean generated caches and uninstall the template from the isolated environment.

Any failure fails CI. Diagnostic logs are uploaded as `verification-<os>` artifacts.
All preview server processes started by verification are stopped, including on failure.

## Add or update a template

- Put each template in `content/<name>/` with `.template.config/template.json`.
  Give it a unique `identity` and `shortName`, and set `sourceName` to the project name.
- Keep generated projects self-contained: do not depend on repository-wide MSBuild
  settings, unpublished packages, local feeds, or a sibling Kiji checkout.
- Add a case to `scripts/test-cases.json` with its directory, short name, and expected
  home-page text. The shared checks cover every registered template; extend them when
  a template introduces new output or options.
- Update the concrete Kiji `PackageReference` in each template when adopting a new
  library version. Dependabot proposes these updates. CI restores from NuGet.org
  into a fresh cache, so an unavailable version blocks release.

Template package versions and Kiji library versions are independent. The package
version lives in `Kiji.Templates.csproj`; Kiji versions live in generated project
sources. Updating a template does not require a Kiji library release.

## Release

One-time setup in the `zzzkan/Kiji.Templates` repository and NuGet account:

1. Add the GitHub Actions secret `NUGET_USER` containing the NuGet.org **profile name**.
2. Create a [NuGet trusted-publishing policy](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
   for owner `zzzkan`, repository `Kiji.Templates`, workflow file `release.yml`.
   Leave the environment empty, matching the workflow. Scope it to `Kiji.Templates`
   and allow publication of a new package for the first release as well as new versions.

For each release:

1. Set `PackageVersion` in `Kiji.Templates.csproj` to the next version (for example,
   `0.1.0` or `0.2.0-preview.1`). Review template dependencies and run verification.
2. Merge the changes to `main` after CI passes on Windows and Linux.
3. Tag that commit with the exact version and push the tag:

   ```sh
   git tag v0.1.0
   git push origin v0.1.0
   ```

`release.yml` rejects a tag that differs from `PackageVersion`, packs once, and tests
that artifact on Windows and Linux. Only after both jobs succeed does it obtain a
short-lived NuGet credential and publish **the same package**, without repacking.
The package artifact and verification logs include enough information to reproduce
a failure locally; each verification records the package SHA-256.

If publishing fails after verification, fix the account/policy configuration and rerun
the failed job. Duplicate versions are skipped. To change package contents after a
successful publication, increment the version and create a new tag.

After NuGet indexing completes, check installation in a clean environment with
`dotnet new install Kiji.Templates@0.1.0` and create/publish a site. For a preview,
specify its full version. Kiji's repository owns the library and documentation;
template development, testing, and publishing are owned here.

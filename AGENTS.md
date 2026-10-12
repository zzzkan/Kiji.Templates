# Kiji.Templates

This repository owns the `Kiji.Templates` NuGet package and its `dotnet new` templates.
The Kiji library and user documentation live in https://github.com/zzzkan/kiji.

- `content/<name>/` contains self-contained template sources. Keep the initial
  template minimal; do not add features unless the task calls for them.
- `Kiji.Templates.csproj` owns the template package version. Each template project
  pins its own published Kiji version. These versions are independent.
- Use the SDK in `global.json` and PowerShell 7.3+. Run
  `pwsh -NoProfile -File scripts/verify.ps1` after template or packaging changes.
  It packs and verifies real generated consumers in an isolated CLI/NuGet environment.
  Logs and fixtures are retained in ignored `artifacts/verification/`.
- To test an existing package, pass `-PackagePath <nupkg>` to `scripts/verify.ps1`.
  Add an entry to `scripts/test-cases.json` for each new template and extend the
  verification for its specific behavior as needed.
- `pwsh scripts/pack.ps1` requires an empty output directory. Do not include build
  outputs or local feeds in templates. Generated projects must work outside this repo.
- `release.yml` validates the `v<PackageVersion>` tag, tests the same artifact on
  Windows and Linux, then publishes it through NuGet trusted publishing.
  Preserve the guarantee that publishing never repacks the tested artifact.
- Stop all watch/server processes started by verification, including on failure.
  Keep README.md and generated READMEs focused on template users. Keep development,
  CI, and release procedures in CONTRIBUTING.md synchronized with the workflows.

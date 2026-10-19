# Kiji.Templates

This repository owns the `Kiji.Templates` NuGet package and its `dotnet new` templates.
The Kiji library and user documentation live in https://github.com/zzzkan/kiji.

Use [verify-kiji-template](.agents/skills/verify-kiji-template/SKILL.md) for verification
and failure triage.

- Keep `src/Kiji.Templates/content/Kiji.Minimal/` minimal and self-contained, with a
  pinned, published Kiji dependency. Do not include build output, local feeds,
  or repository-only settings.
- Name templates `Kiji <Kind>`, with CLI names `kiji-<kind>`, identities and source
  project names `Kiji.<Kind>`, and directories `src/Kiji.Templates/content/Kiji.<Kind>/`.
  `Kiji.Templates` is the package and solution name for the collection.
- `src/Kiji.Templates/Kiji.Templates.csproj` follows the official `templatepack` layout.
  MinVer derives the package version from `v<SemVer>` tags.
  The template's Kiji version and template package version are independent.
- Use the SDK in `global.json`. After template or packaging changes, use the
  verification skill to validate the package, generate projects, and publish
  the generated site. Verify an existing package without repacking it.
  Keep test work directories and diagnostics under `TestResults/`.
  Use `artifacts/demos` for demo work and `dist` for the published Pages site.
  Use the standard NuGet and Playwright caches; do not require shell-wide
  environment overrides for verification. Use the SDK's default package output;
  tests read `src/Kiji.Templates/bin/<Configuration>` for their build configuration.
  Override the package only when needed through `dotnet test --environment
KIJI_TEMPLATE_PACKAGE=<nupkg-or-directory>`.
- `Kiji.Templates.Tests` discovers templates from the package and checks generation,
  publishing, and browser behavior. Extend only the relevant behavior checks when
  adding template features; generated-file snapshots are not used.
- `release.yml` requires a successful main push CI run for the tagged commit,
  then packs once and publishes through NuGet trusted publishing. Linux
  tests run in CI and are not repeated during release.
- Stop every preview process started by verification, including on failure.
- Keep README.md and generated READMEs limited to template usage. Keep verification
  instructions in the skill synchronized with the workflows.

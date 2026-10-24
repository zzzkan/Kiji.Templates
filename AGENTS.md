# Kiji.Templates

This repository owns the `Kiji.Templates` NuGet package and its `dotnet new` templates.

Use [verify-kiji-template](.agents/skills/verify-kiji-template/SKILL.md) for verification
and failure triage.

- Keep `src/Kiji.Templates/content/Kiji.Minimal/` minimal and self-contained, with a
  pinned, published Kiji dependency. Do not include build output, local feeds,
  or repository-only settings.
- Name templates `Kiji <Kind>`, with short names `kiji-<kind>`, identities and source
  project names `Kiji.<Kind>`, and directories `src/Kiji.Templates/content/Kiji.<Kind>/`.
  `Kiji.Templates` is the package and solution name for the collection.
- `src/Kiji.Templates/Kiji.Templates.csproj` follows the official `templatepack` layout.
  MinVer derives the package version from `v<SemVer>` tags. The Kiji version that
  the templates depend on is independent of the template package version.
- Use the SDK in `global.json`. After template or packaging changes, use the
  verification skill to validate the package, generate projects, and publish
  the generated sites. To verify an existing package, do not repack it.
  Keep test work directories and diagnostics under `TestResults/`.
  Use `artifacts/demos` for demo work and `dist` for the published Pages site.
  Use the standard NuGet and Playwright caches, and do not require shell-wide
  environment overrides for verification. Keep the SDK's default package output:
  tests read the package from `src/Kiji.Templates/bin/<Configuration>`, matching
  their own build configuration. Override the package only when needed, with
  `dotnet test --environment KIJI_TEMPLATE_PACKAGE=<nupkg-or-directory>`.
- `Kiji.Templates.Tests` discovers templates from the package and checks generation,
  publishing, and browser behavior. When adding template features, extend only the
  relevant behavior checks. The tests do not use generated-file snapshots.
- `release.yml` requires a successful CI run from a push to `main` for the tagged
  commit, then packs once and publishes through NuGet trusted publishing. Linux
  tests run in CI and are not repeated during release.
- Stop every preview process started by verification, even on failure.
- Keep README.md and generated READMEs limited to template usage. Keep the
  verification instructions in the skill synchronized with the workflows.

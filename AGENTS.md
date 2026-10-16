# Kiji.Templates

This repository owns the `Kiji.Templates` NuGet package and its `dotnet new` templates.
The Kiji library and user documentation live in https://github.com/zzzkan/kiji.

Use [verify-kiji-template](.agents/skills/verify-kiji-template/SKILL.md) for verification
and failure triage.

- Keep `content/minimal/` minimal and self-contained, with a pinned, published Kiji
  dependency. Do not include build output, local feeds, or repository-only settings.
- Name templates `Kiji <Kind>`, with CLI names `kiji-<kind>`, identities and source
  project names `Kiji.<Kind>`, and directories `content/<kind>/`.
  `Kiji.Templates` is the package and solution name for the collection.
- `Kiji.Templates.csproj` follows the official `templatepack` layout.
  MinVer derives the package version from `v<SemVer>` tags.
  The template's Kiji version and template package version are independent.
- Use the SDK in `global.json`. After template or packaging changes, use the
  verification skill to validate the package, compare generated files, and publish
  the generated site. Verify an existing package without repacking it.
  Keep local CLI/NuGet state and generated sites in `artifacts/verification/`.
- Review generated-file changes in `tests/snapshots/` deliberately; never automatically
  accept failing snapshots. Extend verification when adding template behavior.
- `release.yml` tests the same artifact on Windows and Linux before NuGet trusted
  publishing. Publishing must never repack it.
- Stop every preview process started by verification, including on failure.
- Keep README.md and generated READMEs limited to template usage. Keep verification
  instructions in the skill synchronized with the workflows.

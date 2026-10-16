---
name: verify-kiji-template
description: Verify Kiji.Templates changes or an existing NuGet package, and diagnose template snapshot, packaging, or generated-site failures in this repository.
---

# Verify Kiji.Templates

Use the SDK in `global.json` and PowerShell 7.3+. After template or packaging
changes, run the checks below; `dotnet pack` alone does not validate the template.

In a fresh PowerShell session at the repository root, prepare isolated CLI/NuGet
state and a new output directory:

```pwsh
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$run = Join-Path $PWD "artifacts/verification/$([Guid]::NewGuid().ToString('N'))"
$env:DOTNET_CLI_HOME = "$run/cli"
$env:NUGET_PACKAGES = "$run/packages"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = '0'
dotnet tool restore
```

Pack the template, or skip this block and set `$package` to the absolute path of an
existing `.nupkg` to verify it without repacking:

```pwsh
dotnet pack Kiji.Templates.csproj -c Release -o "$run/packed"
$package = (Get-Item "$run/packed/*.nupkg").FullName
```

Run the checks shared by [CI](../../../.github/workflows/ci.yml) and
[releases](../../../.github/workflows/release.yml):

```pwsh
dotnet tool run dotnet-template-authoring -- validate $package
dotnet tool run dotnet-template-authoring -- verify kiji-minimal `
    --template-path $package `
    --scenario-name named --template-args "-n Travel.Journal --no-restore" `
    --output "$run/generated site" --snapshots-directory tests/snapshots `
    --disable-diff-tool --disable-default-exclude-patterns
dotnet new install $package
dotnet new kiji-minimal -o "$run/MySite"
if (!(Test-Path "$run/MySite/MySite.csproj") -or
    (Get-Content "$run/MySite/Program.cs" -Raw) -notmatch 'Name = "MySite"' -or
    (Get-Content "$run/MySite/README.md" -Raw) -notmatch '^# MySite\r?\n') {
    throw 'The output directory name was not used as the project and site name.'
}
dotnet new kiji-minimal -n Travel.Journal -o "$run/named site" --no-restore
if (!(Test-Path "$run/named site/Travel.Journal.csproj") -or
    (Get-Content "$run/named site/Program.cs" -Raw) -notmatch 'Name = "Travel.Journal"' -or
    (Get-Content "$run/named site/README.md" -Raw) -notmatch '^# Travel\.Journal\r?\n') {
    throw 'The explicit project and site name did not override the output directory name.'
}
if ((Test-Path "$run/generated site/kiji-minimal/obj/project.assets.json") -or
    (Test-Path "$run/named site/obj/project.assets.json")) {
    throw '--no-restore did not skip dependency restore.'
}

New-Item -ItemType Directory -Force "$run/logs" | Out-Null
dotnet publish "$run/MySite/MySite.csproj" -c Release --no-restore "-bl:$run/logs/standard-publish.binlog"
if ((Get-Content "$run/MySite/dist/index.html" -Raw) -notmatch 'Hello from Kiji') {
    throw 'Published home page is missing its expected content.'
}
```

Use fresh output directories. Publishing with `--no-restore` checks that project
creation restored dependencies successfully. The snapshot covers explicit naming
and `--no-restore`; the separate CLI checks cover directory-based naming and an
explicit name overriding a different output directory, including README headings.

For snapshot failures, compare `tests/snapshots/*.received/` with the reviewed
`.verified` baseline. Apply only intentional changes; never bulk-accept differences
or copy build output. For publish failures, inspect `$run/logs/standard-publish.binlog`.
CI uploads snapshot differences and publish logs as `verification-<os>` on failure.

Choose additional checks based on the change:

- Naming or restore options: extend the CLI checks here and in CI together.
- Package layout or exclusions: inspect package contents and metadata; test
  directory installation if its exclusions changed.
- Preview or clean behavior: run `dotnet watch` or `dotnet clean` in the generated
  site. Check the served home page and stop every preview process, including on failure.
- Demo publishing: follow the Pages checks below.

## GitHub Pages demos

[Deploy demos](../../../.github/workflows/pages.yml) packs the current templates,
generates each demo, and publishes all demos as one Pages artifact. Pull requests
build only; pushes to `main` and manual runs on `main` deploy. Set the repository's
Pages source to **GitHub Actions** before the first deployment. The workflow reads
the configured Pages URL, including a custom domain when present.

Add template kinds to `-Templates` in the workflow. Each template must generate a
single project with `BaseUrl = new Uri("https://example.com/")` in `Program.cs`.
The demo builder replaces that placeholder only in generated projects, publishes
each site under `<kind>/`, and creates a root index linking to every demo.

After the standard checks above, reuse the installed package and isolated environment:

```pwsh
./.github/scripts/build-demos.ps1 `
    -BaseUrl 'https://zzzkan.github.io/Kiji.Templates/' -Templates @('minimal') `
    -WorkDirectory "$run/demos" -OutputDirectory "$run/pages"
if ((Get-Content "$run/pages/minimal/index.html" -Raw) -notmatch 'Hello from Kiji' -or
    (Get-Content "$run/pages/index.html" -Raw) -notmatch 'href="./minimal/"' -or
    (Get-Content "$run/demos/minimal/Program.cs" -Raw) -notmatch
        'https://zzzkan.github.io/Kiji.Templates/minimal/') {
    throw 'Demo content, index link, or deployment URL is incorrect.'
}
```

Use fresh work and output directories for every build. When adding a template,
include its kind in the local check and inspect its generated links and assets
under its deployment path. Demo publish logs are in `$run/demos/logs/`; the workflow
uploads them as `demo-build-logs` on failure.

Report checks completed and failures. Remove disposable caches and generated sites
when no longer needed, preserving useful failure evidence.

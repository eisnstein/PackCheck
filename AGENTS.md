# AGENTS.md

PackCheck is a .NET global tool (`packcheck`, published on NuGet as `PackCheck`) that checks installed NuGet package versions against nuget.org and can upgrade them in place. Built on Spectre.Console.Cli and NuGet.Protocol.

## Commands

```sh
dotnet build                                   # builds src + tests (PackCheck.slnx)
dotnet test                                    # TUnit on Microsoft.Testing.Platform (see global.json)
dotnet run --project src --framework net10.0   # run locally; add args after `--`, e.g. -- check --pre
dotnet pack src/PackCheck.csproj -c Release    # outputs to src/nupkg
```

- The tool multi-targets `net8.0;net9.0;net10.0`, so `dotnet run` needs `--framework`. Tests target `net10.0` only.
- SDK is pinned to 10.0.100 (`rollForward: latestFeature`).
- `TreatWarningsAsErrors` and `Nullable` are on for every project (`Directory.Build.props`). A nullable warning fails the build.
- Package versions are managed centrally in `Directory.Packages.props`. `<PackageReference>` items in the csproj files have no `Version` attribute.

## Architecture

`src/Program.cs` registers two commands. `check` (alias `c`) is the default; the other is `upgrade` (alias `u`). In DEBUG builds, exceptions propagate and examples are validated.

**Commands** (`src/Commands/`): `CheckCommand` and `UpgradeCommand` contain the whole control flow. Both decide what to operate on in this fixed order:

1. Explicit CLI path: `--csprojFile`, `--slnFile`, `--slnxFile`, `--cpmFile`, `--fbaFile`
2. Auto-detected `Directory.Packages.props` in the current directory (Central Package Management)
3. Auto-detected `*.sln`, then `*.slnx`, in the current directory
4. A single `*.csproj` in the current directory

Each target type has its own private method (`CheckProject`, `CheckCpm`, `CheckFba`, `UpgradeProject`, …). These methods are near-copies of each other. A behaviour change usually has to go into all three variants in a command, and often into both commands.

**Pipeline per target:** read packages from the file → `PackagesService.ApplySettings` (filter/exclude, wildcards `*`/`%`) → `NuGetPackagesService` fetches versions in parallel (max 4) with a progress bar → `PackagesService.CalculateUpgradeType` (check) or `PreparePackagesForUpgrade` (upgrade) → `OutputService` renders the table, or the file service writes the new versions.

**Services** (`src/Services/`) are mostly static classes, one for each file format:
- `CsProjFileService`: XML `<PackageReference Include Version>`, rewritten with `XElement` (whitespace preserved).
- `CentralPackageMgmtService`: `<PackageVersion>` in `Directory.Packages.props`.
- `SolutionFileService`: parses `Project(...)` lines in `.sln` with a regex and converts `\` to the platform separator outside Windows. `SolutionXFileService` handles `.slnx`.
- `FileBasedAppService`: `#:package Name@Version` directives in a single `.cs` file.
- `NuGetApiService` (behind `INuGetApiService`): the only network access. The commands get it through their constructors; `Program.cs` registers it with Spectre's type registrar. It queries nuget.org v3 `PackageMetadataResource` with `includeUnlisted: false` and falls back to `FindPackageByIdResource`.
- `NuGetVersionService`: picks latest stable, latest prerelease and latest overall.
- `PackCheckService`: on every `check` run, tells the user when a newer PackCheck version exists on NuGet.
- `PackCheckConfigService` / `SettingsService`: load `.packcheckrc` or `.packcheckrc.json` from the cwd (case-insensitive JSON → `Data/Config`). They merge it into the settings, and CLI values take precedence via `??=`.

**Version "lanes":** a package whose current version is a prerelease is compared against prereleases, even when the target is "stable". A package on a stable version gets prereleases only with `--target latest` or `--pre`. This logic is in `PackagesService` and is covered by tests. Keep it consistent between check and upgrade.

**Data** (`src/Data/`): `Package` is mutable and gets filled in step by step along the pipeline. `Target` holds the string constants `stable`/`latest`. `Result` is Success/Warning/Error: Warning skips a project in a solution, Error aborts with exit code -1.

**Errors:** the file services throw specific exceptions (`src/Exceptions/`) whose messages contain Spectre markup (`[white]...[/]`). The commands catch them and print `[dim]WARN:[/] [red]{message}[/]`. Escape user-provided strings before you embed them in markup.

## Tests

- TUnit + Verify (snapshot) + `Spectre.Console.Cli.Testing` (`CommandAppTester`).
- Tests run **sequentially** (`[assembly: NotInParallel]`) because they copy fixtures from `tests/TestData/` into the test output directory (the cwd) through `TestHelper.Load*/Delete*`, and the services read from cwd. A new test that touches files has to use the same Load/Delete pattern in `[Before(Test)]`/`[After(Test)]`.
- Tests never hit the network. Create command testers with `TestHelper.CreateCommandAppTester()`, never `new CommandAppTester()`. It registers `tests/Fakes/FakeNuGetApiService.cs`, which returns fixed versions for the packages in `TestData/test.csproj`, through `TestTypeRegistrar`. Spectre's `FakeTypeRegistrar` can't construct commands with constructor parameters. When you change the fake's versions, re-record the `CheckCommand` snapshots in `tests/Snapshots/`.
- For logic tests, build packages with `tests/Factories/PackageFactory.cs`.
- `.cs` files under `TestData/` are excluded from compilation, because they are fixtures.
- Snapshot files (`*.verified.txt`) are UTF-8 with BOM and have no final newline (see `.editorconfig`).

## Conventions

- File-scoped namespaces, Allman braces, 4-space indent, LF line endings. Csproj, props, JSON and YAML files use 2 spaces.
- Use `var` when the type is apparent and explicit types otherwise. `.editorconfig` treats `csharp_style_var_elsewhere = false` as an error.
- Every member needs an explicit accessibility modifier.
- Console output goes through Spectre (`AnsiConsole` / injected `IAnsiConsole`). Prefixes follow the `[dim]INFO:[/]` / `[dim]WARN:[/]` style.
- When you add a CLI option: add it to `CommonSettings` (or `CheckSettings`/`UpgradeSettings`), add the property to `Data/Config.cs` if it should be configurable, merge it in **both** overloads of `SettingsService.CombineSettingsWithConfig`, and update `README.md`.
- Update `CHANGELOG.md` for user-visible changes. Format: `## x.y.z (YYYY-MM-DD)` with an `### Enhancements` / `### Fixes` section. Credit contributors and link the issue.

## Release

Push a tag `vX.Y.Z`. `.github/workflows/release.yml` builds and packs with `/p:Version` taken from the tag, pushes to NuGet via OIDC (`NuGet/login`), and creates a GitHub release. The version is **not** stored in the csproj. The test step in that workflow is currently commented out, so run `dotnet test` locally before tagging.
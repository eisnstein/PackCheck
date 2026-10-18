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

**Commands** (`src/Commands/`): `CheckCommand` and `UpgradeCommand` derive from `PackageCommand<TSettings>`, which holds the whole shared flow:

1. `--version` (both commands).
2. Build the initial state, a `PackCheckContext<TSettings>` (`src/Data/`), once per run with `PackCheckContextBuilder`:
   - `LoadSettings` reads the config and merges it into the CLI settings. Then the `ValidateSettings` hook (upgrade: no `-i` with json/markdown) and the `BeforeRunAsync` hook (check: self-update notice) run.
   - `ResolveFiles` decides what to work on, in this fixed order: an explicit path (`--csprojFile`, `--slnFile`, `--slnxFile`, `--cpmFile`, `--fbaFile`, or the same from the config), then an auto-detected `Directory.Packages.props`, then `*.sln`, then `*.slnx`, then a single `*.csproj` in the current directory. Every file, including all projects of a solution, is checked to exist before anything is processed.
3. For each file: print `DescribeFile` → read packages → `PackagesService.ApplySettings` (filter/exclude, wildcards `*`/`%`) → `SelectPackages` hook (upgrade: the package name argument) → `NuGetPackagesService` fetches versions in parallel (max 4) with a progress bar → `ProcessPackagesAsync` (check: upgrade type and table; upgrade: target versions, optional prompt, write).
4. `AfterRun` hook (check: the INFO lines), or the structured document (see below).

The commands only implement the hooks. Behaviour that applies to both belongs in `PackageCommand` or `PackCheckContextBuilder`.

**Structured output** (`--output json|markdown`, `Data/OutputFormat`): stdout must contain nothing but one document, also when stdout and stderr are merged.
- Status lines go through `PackageCommand.Status`, which is silent in that mode. The progress bar, the self-update notice and the dry-run file contents are skipped.
- `ProcessPackagesAsync` passes its result to `Report` instead of printing it. Files with nothing to do are reported with an empty list.
- At the end, or on an error, `PackageCommand` calls `WriteStructuredOutput`, which uses `StructuredOutputService`. Errors become a plain-text `error` field (`Markup.Remove`).
- The document is written to `Output.Profile.Out.Writer`, not with `Output.WriteLine`, because Spectre wraps long lines at the console width.
- New output in a command must go through `Status`, or be guarded by `IsStructuredOutput`.

**Package files** (`src/PackageFiles/`): `IPackageFile` (`Path`, `ReadPackagesAsync`, `WriteVersionsAsync`) with `CsProjPackageFile`, `CpmPackageFile` and `FbaPackageFile`. They are thin wrappers around the file services below, so the commands never need to know which kind of file they work on.

**Output:** all command output goes through the injected `IAnsiConsole` (`PackageCommand.Output`), so tests see it. The exceptions are the NuGet progress bar and the raw dry-run output of the file services.

**Services** (`src/Services/`) are mostly static classes, one for each file format:
- `CsProjFileService`: XML `<PackageReference Include Version>`.
- `CentralPackageMgmtService`: `<PackageVersion>` in `Directory.Packages.props`.
- `PackageXmlFileService`: does the upgrade for both. It uses `XmlReader` only to locate each `Version` attribute and replaces just that text in the original content. Never re-serialize the XML, because that changes the declaration, BOM, line endings, final newline and attribute layout. `XmlFormattingTests` checks this byte for byte.
- `SolutionFileService`: parses `Project(...)` lines in `.sln` with a regex and converts `\` to the platform separator outside Windows. `SolutionXFileService` handles `.slnx`.
- `FileBasedAppService`: `#:package Name@Version` directives in a single `.cs` file.
- `NuGetApiService` (behind `INuGetApiService`): the only network access. The commands get it through their constructors; `Program.cs` registers it with Spectre's type registrar. It queries nuget.org v3 `PackageMetadataResource` with `includeUnlisted: false` and falls back to `FindPackageByIdResource`.
- `NuGetVersionService`: picks latest stable, latest prerelease and latest overall.
- `PackCheckService`: on every `check` run, tells the user when a newer PackCheck version exists on NuGet.
- `PackCheckConfigService` / `SettingsService`: load `.packcheckrc` or `.packcheckrc.json` from the cwd (case-insensitive JSON → `Data/Config`). They merge it into the settings, and CLI values take precedence via `??=`.

**Version "lanes":** `PackagesService.GetStableTargetVersion` is the single definition of "stable". The "Latest Stable" column, the check result and `upgrade --target stable` all use it. On a stable version it is the latest stable. On a prerelease it is the latest stable if that is newer (e.g. `3.0.0-rc.1` → `3.0.0`), otherwise the latest prerelease. A package on a stable version gets prereleases only with `--target latest` or `--pre`. Upgrades only go to higher versions, never down. All of this is covered by tests in `PackagesServiceTest`.

**Data** (`src/Data/`): `Package` is mutable and gets filled in step by step along the pipeline. `Target` holds the string constants `stable`/`latest`. `Result` is Success/Warning/Error. A Warning ("nothing to do", e.g. no packages or all filtered out) does not stop the run and leads to exit code 0. An Error aborts with exit code -1. Exit code -1 is only for real errors: missing or ambiguous file, invalid config, feed unreachable.

**Errors:** the file services throw specific exceptions (`src/Exceptions/`) whose messages contain Spectre markup (`[white]...[/]`). `PackageCommand.ExecuteAsync` catches them and prints `[dim]WARN:[/] [red]{message}[/]`. Escape user-provided strings before you embed them in markup.

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
- When you add a CLI option: add it to `CommonSettings` (or `CheckSettings`/`UpgradeSettings`), add the property to `Data/Config.cs` if it should be configurable, merge it in `SettingsService.CombineSettingsWithConfig` (options that exist only for one command go in its `is CheckSettings` part), and update `README.md`.
- Add user-visible changes to `CHANGELOG.md` under `## Unreleased`, in the sections `### Breaking Changes` / `### Enhancements` / `### Bug fixes` (in that order, leave out empty ones). Credit contributors and link the issue.

## Release

1. In `CHANGELOG.md`, rename `## Unreleased` to `## X.Y.Z (YYYY-MM-DD)` and add a new empty `## Unreleased` above it. Commit.
2. Push a tag `vX.Y.Z`.

`.github/workflows/release.yml` then runs these steps:
- It first checks that `CHANGELOG.md` has a `## X.Y.Z ` section and stops otherwise, before anything is published. A version on NuGet cannot be replaced.
- It builds, tests and packs with `/p:Version` taken from the tag. The version is **not** stored in the csproj.
- It pushes to NuGet via OIDC (`NuGet/login`).
- It creates the GitHub release. The release notes are the install command, the version's changelog section and a link to the commits since the previous tag. Tags with a `-` (e.g. `v5.0.0-rc.1`) become prereleases.
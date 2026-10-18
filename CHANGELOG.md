# Changelog

## 4.0.1 (2026-10-18)

### Bug fixes

- Show a clear warning instead of an exception when nuget.org cannot be reached, e.g. when offline.
- `upgrade` now keeps the BOM of file-based app files.
- Paths with `[` or `]` in warnings and messages no longer break the output.
- An invalid `.packcheckrc` config file now shows the intended warning instead of an error about the warning itself.

## 4.0.0 (2026-10-18)

### Breaking Changes

- `filter`, `exclude` and the package name given to `upgrade` now ignore case, like NuGet package IDs do. For example, `-x newtonsoft.json` now excludes `Newtonsoft.Json`.
- `upgrade` now uses the `CsProjFile`, `SlnFile`, `SlnxFile` and `CpmFile` paths from the `.packcheckrc` config file, like `check` already did. Before, it ignored them when choosing what to upgrade.
- If the installed version is a prerelease, the _latest stable_ column now shows the newest prerelease when there is no newer stable version yet. This is the version `upgrade` moves to.

### Enhancements

- `upgrade` now only changes the version numbers. The XML declaration, BOM, line endings, final newline and attribute layout of `.csproj` and `Directory.Packages.props` files stay as they were.
- `upgrade` no longer changes the indentation and line endings of file-based apps.
- Versions that are not plain versions, like `$(FooVersion)`, `[1.0,2.0)` or `1.*`, are skipped instead of stopping PackCheck.
- Update dependencies

### Bug fixes

- A prerelease is now offered its stable release, e.g. `3.0.0-rc.1` -> `3.0.0`.
- `upgrade` never moves a package to a lower version, e.g. when the installed version was unlisted.
- `upgrade` no longer crashes when a package is referenced more than once, e.g. in conditional `ItemGroup`s per target framework. In interactive mode only the selected entry is upgraded.
- PackCheck no longer crashes when the check for a new PackCheck version fails, e.g. when offline.
- A missing or ambiguous `.sln`, `.slnx`, `Directory.Packages.props` or file-based app file now shows a warning instead of crashing.
- `FbaFile` from the config file is now used.
- Projects in `.slnx` files are now found on Linux and macOS.
- Projects of a solution are now found relative to the solution file, so `--slnFile`/`--slnxFile` (or `SlnFile`/`SlnxFile` in the config file) can point to a solution in another directory.

## 3.2.0 (2026-07-23)

### Enhancements

- Add wildcard support for `filter` and `exclude` settings and options using `*` or `%` (for example `Telerik.*`) to match package name patterns. Thanks to @zgabi. (https://github.com/eisnstein/PackCheck/issues/6)
- Update dependencies

## 3.1.5 (2026-04-17)

### Enhancements

- Update dependencies

## 3.1.4 (2026-02-08)

### Enhancements

- Show prerelease version if installed version is prerelease. Thanks to @zgabi. (https://github.com/eisnstein/PackCheck/issues/4)
- Only use listed package versions. Thanks to @zgabi. (https://github.com/eisnstein/PackCheck/issues/5)
- Update dependencies

## 3.1.3 (2026-01-29)

### Enhancements

- Only show packages if versions differ
- Add `--version` flag to get the current installed version of PackCheck
- Update dependencies

## 3.1.2 (2025-12-21)

### Enhancements

- Make HTTP requests to the NuGet registry in parallel
- Update dependencies

## 3.1.1 (2025-12-13)

### Enhancements

- Remove the word _Version_ from the header in the columns to make them more narrow.
- Update dependencies

## 3.1.0 (2025-12-07)

### Enhancements

- Add the `--pre` flag to the `check` command which defines if the `latest` version column should be shown. Default is `false`. You can also add the `"Pre": true` to your `.packcheckrc` file to show prerelease versions.
- Switch from `xUnit` to `TUnit` test framework
- Update dependencies

## 3.0.3 (2025-11-14)

### Enhancements

- Add `net10.0` to target frameworks
- Update dependencies

## 3.0.2 (2025-10-12)

### Enhancements

- Remove an unnecessary while loop in the fetch packages info task
- Update dependencies

### Bug fixes

- Prevent freeze when more than 100 packages are given. Thanks to @zgabi. (https://github.com/eisnstein/PackCheck/pull/3)

## 3.0.1 (2025-09-12)

### Enhancements

- Add GitHub workflow for releasing to NuGet

### Breaking Changes

- Remove `net6.0` and `net7.0` from target frameworks

## 2.4.1 (2025-09-12)

### Enhancements

- Update dependencies

### Bug fixes

- If the `format` option is set to **group**, no output was shown if _current_ and _stable_ version were equal.

## 2.4.0 (2025-06-07)

### Enhancements

- Add support for [file-based apps](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)

## 2.3.1 (2025-06-03)

### Bug fixes

- Correctly use and discover of `.slnx` file

## 2.3.0 (2025-05-21)

### Enhancements

- Add support for new solution file format `.slnx`

## 2.2.0 (2024-11-19)

### Enhancements

- Add `net9.0` to target frameworks
- New `format` option to group packages by _patch_, _minor_ or _major_ upgrade.

## 2.1.0 (2024-07-07)

### Bug fixes

- Add missing `filter` and `exclude` settings to `upgrade` command

## 2.0.0 (2024-05-20)

### Enhancements

- Introduce the PackCheck config file - `.packcheckrc` or `.packcheckrc.json` - to control the behaviour of PackCheck
- New `filter` and `exclude` options to filter packages
- Update dependencies

### Breaking Changes

- Remove `net5.0` from target frameworks

## 1.0.1 (2023-12-27)

### Enhancements

- Add link to CHANGELOG.md in PackCheck outdated info

## 1.0.0 (2023-12-26)

### Breaking Changes

- Rename `--version` option on the `upgrade` command to `--target`

### Enhancements

- Add `net8.0` to target frameworks

## 0.4.0 (2023-08-05)

### Enhancements

- Add support for Central Package Management

## 0.3.2 (2023-04-21)

### Enhancements

- Add info if version of PackCheck itself is outdated

## 0.3.1 (2022-11-20)

### Enhancements

- Add `net7.0` to target frameworks

## 0.3.0 (2022-10-15)

### Enhancements

- [packcheck upgrade -i] Update interactive mode to show a list of all updatable packages and make each selectable for upgrade
- Add aliases to `check` and `upgrade` commands. You can now write `packcheck u` to upgrade

Note: The fetching of package data from NuGet is now done serially and not concurrently anymore. That code was bit hacky and I wanted a saver version of it. The `check` now takes longer time.

## 0.2.3 (2022-10-04)

### Bug fixes

- Replace backslash with forward slash on unix like environments in paths to projects from project definitions in solution file

## 0.2.2 (2022-09-21)

### Bug fixes

- Now really clear list of packages for each project to avoid showing wrong data in output

## 0.2.1 (2022-09-18)

### Bug fixes

- Update readme file

## 0.2.0 (2022-09-18)

### Bug fixes

- Clear list of packages for each project to avoid showing wrong data

## 0.2.0-preview.1 (2022-09-02)

### Enhancements

- Add functionality to run the `check` and `upgrade` command in a solution

## 0.1.1 (2021-10-11)

### Enhancements

- Change release notes to URL to CHANGELOG.md file

## 0.1.0 (2021-10-10)

### Enhancements

- [packcheck upgrade] Add interactive mode (-i|--interactive)

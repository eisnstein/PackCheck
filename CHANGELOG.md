# Changelog

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

# Changelog

## 0.3.2 (2023-04-21)

### Enhancements
* Add info if version of PackCheck itself is outdated

## 0.3.1 (2022-11-20)

### Enhancements
* Add `net7.0` to target frameworks

## 0.3.0 (2022-10-15)

### Enhancements
* [packcheck upgrade -i] Update interactive mode to show a list of all updatable packages and make each selectable for upgrade
* Add aliases to `check` and `upgrade` commands. You can now write `packcheck u` to upgrade

Note: The fetching of package data from NuGet is now done serially and not concurrently anymore. That code was bit hacky and I wanted a saver version of it. The `check` now takes longer time.

## 0.2.3 (2022-10-04)

### Bug fixes
* Replace backslash with forward slash on unix like environments in paths to projects from project definitions in solution file

## 0.2.2 (2022-09-21)

### Bug fixes
* Now really clear list of packages for each project to avoid showing wrong data in output

## 0.2.1 (2022-09-18)

### Bug fixes
* Update readme file

## 0.2.0 (2022-09-18)

### Bug fixes
* Clear list of packages for each project to avoid showing wrong data

## 0.2.0-preview.1 (2022-09-02)

### Enhancements
* Add functionality to run the `check` and `upgrade` command in a solution

## 0.1.1 (2021-10-11)

### Enhancements
* Change release notes to URL to CHANGELOG.md file

## 0.1.0 (2021-10-10)

### Enhancements
* [packcheck upgrade] Add interactive mode (-i|--interactive)

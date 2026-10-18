![PackCheck-Logo](https://raw.githubusercontent.com/eisnstein/PackCheck/main/src/Assets/icon.png)

# PackCheck

[![NuGet Package](https://img.shields.io/nuget/vpre/PackCheck)](https://nuget.org/packages/PackCheck)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://github.com/eisnstein/PackCheck/blob/main/LICENSE)

Check for newer versions of installed NuGet Packages in your Terminal.

---

PackCheck is a dotnet tool for checking versions of installed NuGet packages in your .NET projects in your terminal.
The `check` command (default) shows you all NuGet packages in a nice table with the _current_, _latest stable_ and _latest_ versions of each package.
You can upgrade the .csproj (or Directory.Packages.props if you use Central Package Management) file with the `upgrade` command to your desired target versions. Whether to the _latest stable_ or _latest_ version, only a specific
package or all at once.

## Installation

You can install PackCheck as a dotnet tool via NuGet:

```sh
# Install
dotnet tool install --global PackCheck

# Update
dotnet tool update --global PackCheck
```

## Usage

In your terminal `cd` into a .NET project or .NET solution and run:

```sh
packcheck

# or

packcheck c

# or

packcheck check
```

This should give you something like this:

![PackCheck check example](https://raw.githubusercontent.com/eisnstein/PackCheck/main/src/Assets/packcheck-check-without-pre.png)

If you also want to see the _latest version_, which includes prereleases, use the `--pre` flag:

```sh
packcheck --pre
```

This should give you something like this:

![PackCheck check example](https://raw.githubusercontent.com/eisnstein/PackCheck/main/src/Assets/packcheck-check.png)

To check versions of a file-based app you need to provide the path to the file:

```sh
packcheck --fbaFile app.cs
```

After that you can upgrade the package versions in the _.csproj_ file (or _.csproj_ files in a solution, the _Directory.Packages.props_ file or the given file-based app file) to their corresponding stable versions by running:

> This changes your **.csproj** file(s) or the **Directory.Packages.props** or the provided **file-based app file**!

```sh
packcheck upgrade

# or

packcheck u

# For file-based app you need to provide the path to the file
packcheck u --fbaFile app.cs
```

To upgrade to the latest versions run:

```shell
packcheck upgrade --target latest

# or

packcheck u --target latest
```

For a dry-run, which outputs the _.csproj_ file (or the _Directory.Packages.props_, or the changes of a _file-based app file_) into the terminal without actually changing the file, run:

```sh
packcheck upgrade --dry-run

# or

packcheck u --dry-run
```

To use interactive mode, where you can select each package you want to upgrade, run:

```sh
packcheck upgrade -i

# or

packcheck u -i

# or to upgrade to the latest versions

packcheck u --target latest -i
```

To select packages which should be checked or upgraded, run:

```sh
packcheck --filter "NuGet.Version" -f "Microsoft.Logging"

# or use a wildcard

packcheck --filter "Microsoft.*"
```

To exclude packages which should not be checked or upgraded, run:

```sh
packcheck --exclude "NuGet.Version" -x "Microsoft.Logging"

# or use a wildcard

packcheck --exclude "Microsoft.*"
```

To format the output of the `check` command, use the `--format` option. Currently only `group` is supported, which groups the packages by _patch_, _minor_ and _major_ versions.

```shell
packcheck --format group
```

To show the _latest_ versions in the output of the `check` command, use the `--pre` option, otherwise only the _current_ and _latest stable_ columns will be shown.

```shell
packcheck --pre
```

If the installed version is a prerelease and there is no newer stable version yet, the _latest stable_ column shows the newest prerelease. That is the version `packcheck upgrade` upgrades to.

### Output for scripts and LLM agents

With `--output json` or `--output markdown`, `check` and `upgrade` print only the result, as one document: no progress bar, no status lines and no hints. This makes PackCheck easy to use from scripts, CI jobs and LLM agents.

```shell
packcheck --output json
```

```json
{
  "command": "check",
  "files": [
    {
      "path": "src/App/App.csproj",
      "packages": [
        {
          "name": "NuGet.Protocol",
          "currentVersion": "6.2.1",
          "latestStableVersion": "7.6.0",
          "latestVersion": "7.6.0",
          "upgradeType": "major"
        }
      ]
    }
  ],
  "error": null
}
```

- `check` lists the outdated packages of each file. `upgradeType` is `major`, `minor` or `patch` and follows `--pre` and `--target`, like the table.
- `upgrade` lists the upgraded packages with `currentVersion` and `newVersion`, plus `target` and `dryRun`. With `--dry-run`, no file is changed and the document shows what would change. `--interactive` cannot be combined with `--output json` or `markdown`.
- Paths are relative to the current directory. A file without anything to do has an empty `packages` list.
- On an error, `error` contains the message and the exit code is `-1`. Otherwise `error` is `null` and the exit code is `0`.
- `--output markdown` prints the same as Markdown tables, one section per file, e.g. for pull request comments or job summaries.
- `--format group` only applies to the default `table` output.

## Configuration

You can configure PackCheck via a `.packcheckrc.{json}` file. Example:

```json
{
  "CsProjFile": "path/to/Project.csproj",
  "SlnFile": "path/to/Project.sln",
  "SlnxFile": "path/to/Project.slnx",
  "CpmFile": "path/to/Directory.Packages.props",
  "FbaFile": "path/to/file-based-app.cs",
  "Filter": ["NuGet.Version", "Microsoft.*"],
  "Exclude": ["Microsoft.Logging", "Telerik.*"],
  "Format": "group",
  "Pre": true,
  "Output": "json"
}
```

`Filter` supports wildcard patterns via `*` or `%`, for example `"Microsoft.*"` to include all Microsoft packages.
`Exclude` supports wildcard patterns via `*` or `%`, for example `"Telerik.*"` to exclude all Telerik packages.
Both ignore case, like NuGet package IDs.

For help run:

```sh
packcheck -h
```

## Color Highlighting

| Color  | Description                                    |
| ------ | ---------------------------------------------- |
| red    | Major (Breaking changes)                       |
| yellow | Minor (New features, but backwards compatible) |
| green  | Patch (Backwards compatible bug fixes only)    |

## LICENSE

MIT

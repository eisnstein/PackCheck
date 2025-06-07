![PackCheck-Logo](https://github.com/eisnstein/PackCheck/blob/main/src/Assets/icon.png)

# PackCheck

[![NuGet Package](https://img.shields.io/nuget/vpre/PackCheck)](https://nuget.org/packages/PackCheck)
[![License](https://img.shields.io/github/license/eisnstein/PackCheck)](https://github.com/eisnstein/PackCheck/blob/main/LICENSE)

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

To check versions of a file-based app you need to provide the path to the file:

```sh
packcheck --fbaFile app.cs
```

This should give you something like this:

![PackCheck check example](https://github.com/eisnstein/PackCheck/blob/main/src/Assets/packcheck-check.png)

After that you can upgrade the package versions in the _.csproj_ file (or _.csproj_ files in a solution, the _Directory.Packages.props_ file or the given file-based app file) to their corresponding stable versions by running:

> This changes your **.csproj** file(s) or the **Directory.Packages.props** or the provided **file-based app file**!

```sh
packcheck upgrade

# or

packcheck u

# for file-based app you need to provide the path to the file
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
```

To exclude packages which should not be checked or upgraded, run:

```sh
packcheck --exclude "NuGet.Version" -x "Microsoft.Logging"
```

To format the output of the `check` command, use the `--format` option. Currently only `group` is supported, which groups the packages by _patch_, _minor_ and _major_ versions.

```shell
packcheck --format group
```

## Configuration

You can configure PackCheck via a `.packcheckrc.{json}` file. Example:

```json
{
  "CsProjFile": "path/to/Project.csproj",
  "SlnFile": "path/to/Project.sln",
  "SlnxFile": "path/to/Project.slnx",
  "CpmFile": "path/to/Directory.Packages.props",
  "FbaFile": "path/to/file-based-app.cs",
  "Filter": ["NuGet.Version"],
  "Exclude": ["Microsoft.Logging"],
  "Format": "group"
}
```

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

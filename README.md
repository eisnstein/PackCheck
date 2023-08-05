![PackCheck-Logo](https://github.com/eisnstein/PackCheck/blob/main/src/Assets/icon.png)

# PackCheck

[![NuGet Package](https://img.shields.io/nuget/vpre/PackCheck)](https://nuget.org/packages/PackCheck)
[![License](https://img.shields.io/github/license/eisnstein/PackCheck)](https://github.com/eisnstein/PackCheck/blob/main/LICENSE)

Check for newer versions of installed NuGet Packages in your Terminal.

---

PackCheck is a dotnet tool for checking versions of installed NuGet packages in your .NET projects in your terminal.
The `check` command (default) shows you all NuGet packages in a nice table with the *current*, *latest stable* and *latest* versions of each package.
You can upgrade the .csproj (or Directory.Packages.props if you use Central Package Management) file with the `upgrade` command to your desired versions. Whether to the *latest stable* or *latest* version, only a specific
package or all at once.


## Installation

You can install PackCheck as a dotnet tool via NuGet:

 ```shell
 # Install
 dotnet tool install --global PackCheck
 
 # Update
 dotnet tool update --global PackCheck
 ```

## Usage

In your terminal `cd` into a .NET project or .NET solution and run:

```shell
packcheck

# or

packcheck c

# or

packcheck check
```


This should give you something like this:

![PackCheck check example](https://github.com/eisnstein/PackCheck/blob/main/src/Assets/packcheck-check.png)

After that you can upgrade the package versions in the _.csproj_ file (or _.csproj_ files in a solution, or the _Directory.Packages.props_ file) to their corresponding stable versions by running:
> This changes your **.csproj** file(s) or the **Directory.Packages.props** file!

```shell
packcheck upgrade

# or

packcheck u
```

To upgrade to the latest versions run:

```shell
packcheck upgrade --version latest

# or

packcheck u --version latest
```

For a dry-run, which outputs the _.csproj_ file (or the _Directory.Packages.props_) into the terminal without actually changing the file, run:

```shell
packcheck upgrade --dry-run

# or

packcheck u --dry-run
```

To use interactive mode, where you can select each package you want to upgrade, run:

```shell
packcheck upgrade -i

# or

packcheck u -i

# or to update to the latest versions

packcheck u --version latest -i
```

For help run:

```shell
packcheck -h
```

## Color Highlighting

| Color | Description |
| ----- | ------------ |
| red | Major (Breaking changes) |
| yellow | Minor (New features, but backwards compatible) |
| green | Patch (Backwards compatible bug fixes only) |

## LICENSE

MIT

# PackCheck Tests

### Run tests

```shell
dotnet test
```

### Test the `check` command

```shell
# check command in a solution

packcheck
packcheck c
packcheck check

# check command in a solution - specific project

packcheck --csprojFile src/PackCheck.csproj
packcheck c --csprojFile src/PackCheck.csproj
packcheck check --csprojFile src/PackCheck.csproj
```

### Test the `upgrade` command

```shell
# upgrade command - to stable version

packcheck u
packcheck upgrade

# upgrade command - to latest version

packcheck u --version latest
packcheck upgrade --version latest

# upgrade command - to stable version - interactive

packcheck u -i
packcheck upgrade -i

# upgrade command - to latest version - interactive

packcheck u --version latest -i
packcheck upgrade --version latest -i

# upgrade command - to stable version - a specific package

packcheck u Nuget.Protocol
packcheck upgrade Nuget.Protocol

# upgrade command - to latest version - a specific package

packcheck u Nuget.Protocol --version latest
packcheck upgrade Nuget.Protocol --version latest
```

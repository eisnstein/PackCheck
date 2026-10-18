using System.Xml;
using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Exceptions;

namespace PackCheck.Services;

public static class CsProjFileService
{
    public static string GetPathToCsProjFile(string? pathToCsProjFile = null)
    {
        var cwd = Directory.GetCurrentDirectory();

        // The user provided a path to the .csproj file
        if (!string.IsNullOrEmpty(pathToCsProjFile))
        {
            // If pathToCsProjFile is already a full path, it will not
            // be combined with the current directory.
            var fullPath = Path.Combine(cwd, pathToCsProjFile);
            if (!File.Exists(fullPath))
            {
                throw new CsProjFileException(
                    $"File [white]{pathToCsProjFile}[/] does not exist in the current directory [white]{cwd}[/]"
                );
            }

            return fullPath;
        }

        // No path was provided, we try to find a .csproj file
        var files = Directory.GetFiles(cwd, "*.csproj");

        return files.Length switch
        {
            0 => throw new CsProjFileException($"Could not find a .csproj file in the current directory [white]{cwd}[/]"),
            > 1 => throw new CsProjFileException($"Found more than 1 .csproj file. Please provide the .csproj file to use via the --csprojFile argument. [white]{cwd}[/]"),
            _ => Path.Combine(cwd, files[0])
        };
    }

    public static async Task<List<Package>> GetPackagesDataFromCsProjFileAsync(string pathToCsProjFile)
    {
        List<Package> packages = new();
        var settings = new XmlReaderSettings { Async = true };
        using var reader = XmlReader.Create(pathToCsProjFile, settings);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "PackageReference")
            {
                continue;
            }

            if (!reader.HasAttributes)
            {
                continue;
            }

            var packageName = "";
            var currentVersion = "";

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "Include")
                {
                    packageName = reader.Value;
                }

                if (reader.Name == "Version")
                {
                    currentVersion = reader.Value;
                }
            }

            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(currentVersion))
            {
                continue;
            }

            // Skip values that are not a plain version, e.g. ranges [1.0,2.0),
            // floating versions 1.* or MSBuild properties $(FooVersion)
            if (!NuGetVersion.TryParse(currentVersion, out var version))
            {
                continue;
            }

            packages.Add(new Package(packageName, version));
        }

        return packages;
    }

    public static Task UpgradePackageVersionsAsync(string pathToCsProjFile, IReadOnlyList<Package> packages, bool dryRun)
    {
        return PackageXmlFileService.UpgradePackageVersionsAsync(pathToCsProjFile, "PackageReference", packages, dryRun);
    }
}

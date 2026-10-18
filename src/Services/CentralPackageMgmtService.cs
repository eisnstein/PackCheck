using System.Xml;
using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Exceptions;

namespace PackCheck.Services;

public static class CentralPackageMgmtService
{
    public static readonly string CpmFileName = "Directory.Packages.props";

    public static bool HasCentralPackageMgmt()
    {
        var cwd = Directory.GetCurrentDirectory();
        var cpmFile = Path.Combine(cwd, CpmFileName);
        return File.Exists(cpmFile);
    }

    public static string GetPathToCpmFile(string? pathToCpmFile = null)
    {
        var cwd = Directory.GetCurrentDirectory();

        // A path to the cpm file is given
        if (!string.IsNullOrEmpty(pathToCpmFile))
        {
            // If pathToCpmFile is already a full path, it will not
            // be combined with the current directory.
            var fullPath = Path.Combine(cwd, pathToCpmFile);
            if (!File.Exists(fullPath))
            {
                throw new CpmFileException(
                    $"File [white]{pathToCpmFile}[/] does not exist in the current directory [white]{cwd}[/]"
                );
            }

            return fullPath;
        }

        // No path was provided, we try to find a Directory.Packages.props file
        var cpmFilePath = Path.Combine(cwd, CpmFileName);
        if (!File.Exists(cpmFilePath))
        {
            throw new CpmFileException($"Could not find a {CpmFileName} file in the current directory [white]{cwd}[/]");
        }

        return cpmFilePath;
    }

    public static async Task<List<Package>> GetPackagesDataFromCpmFileAsync(string pathToCpmFile)
    {
        List<Package> packages = new();
        var settings = new XmlReaderSettings { Async = true };
        using var reader = XmlReader.Create(pathToCpmFile, settings);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "PackageVersion")
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

    public static Task UpgradePackageVersionsAsync(string pathToCpmFile, IReadOnlyList<Package> packages, bool dryRun)
    {
        return PackageXmlFileService.UpgradePackageVersionsAsync(pathToCpmFile, "PackageVersion", packages, dryRun);
    }
}

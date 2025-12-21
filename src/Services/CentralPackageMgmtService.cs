using System.Xml;
using System.Xml.Linq;
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
        var reader = XmlReader.Create(pathToCpmFile, settings);

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

            packages.Add(new Package(packageName, NuGetVersion.Parse(currentVersion)));
        }

        reader.Close();

        return packages;
    }

    public static async Task UpgradePackageVersionsAsync(string pathToCpmFile, IReadOnlyList<Package> packages, bool dryRun)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings { Async = true };
        XmlReader reader = XmlReader.Create(pathToCpmFile, readerSettings);

        XElement csProjFile = await XElement.LoadAsync(
            reader, LoadOptions.PreserveWhitespace, CancellationToken.None
        );

        reader.Close();

        // Find all <PackageVersion .../> items
        IEnumerable<XElement> packageVersions = csProjFile
            .Descendants("PackageVersion")
            .Select(el => el);

        foreach (var packageVersion in packageVersions)
        {
            // Get name of current package
            var packageName = packageVersion.Attribute("Include")?.Value;
            if (string.IsNullOrEmpty(packageName))
            {
                continue;
            }

            // Get version of current package
            var currentVersionStr = packageVersion.Attribute("Version")?.Value;
            if (string.IsNullOrEmpty(currentVersionStr))
            {
                continue;
            }

            // Find corresponding package information for the current package reference
            var package = packages.SingleOrDefault(p => p.PackageName == packageName);
            if (package is null)
            {
                continue;
            }

            var currentVersion = NuGetVersion.Parse(currentVersionStr);

            // If no version is available
            // we skip that package
            if (package.NewVersion is null)
            {
                continue;
            }

            // If the current version and new version are equal
            // we dont need to proceed here
            if (currentVersion == package.NewVersion)
            {
                continue;
            }

            // Set the new package version
            packageVersion.SetAttributeValue("Version", package.NewVersion);
        }

        if (dryRun)
        {
            Console.WriteLine(csProjFile);
            return;
        }

        XmlWriterSettings wSettings = new XmlWriterSettings { Async = true, OmitXmlDeclaration = true };
        XmlWriter writer = XmlWriter.Create(pathToCpmFile, wSettings);

        await csProjFile.SaveAsync(writer, CancellationToken.None);
        await writer.FlushAsync();
        writer.Close();
    }
}

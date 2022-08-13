using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NuGet.Versioning;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using Spectre.Console;

namespace PackCheck.Services
{
    public class CsProjFileService
    {
        public string GetPathToCsProjFile(string? pathToCsProjFile = null)
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

        public async Task UpgradePackageVersionsAsync(string pathToCsProjFile, List<Package> packages, UpgradeSettings settings)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings { Async = true };
            XmlReader reader = XmlReader.Create(pathToCsProjFile, readerSettings);

            XElement csProjFile = await XElement.LoadAsync(
                reader, LoadOptions.PreserveWhitespace, CancellationToken.None
            );

            reader.Close();

            // Find all <PackageReference .../> items
            IEnumerable<XElement> packageReferences = csProjFile
                .Descendants("PackageReference")
                .Select(el => el);

            foreach (var packageReference in packageReferences)
            {
                // Get name of current package
                var packageName = packageReference.Attribute("Include")?.Value;
                if (string.IsNullOrEmpty(packageName))
                {
                    continue;
                }

                // Get version of current package
                var currentVersionStr = packageReference.Attribute("Version")?.Value;
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

                // Depending to which target version the user wants to upgrade,
                // select the right package version
                var newVersion = settings.Version switch
                {
                    "stable" => package.LatestStableVersion,
                    "latest" => package.LatestVersion ?? package.LatestStableVersion,
                    _ => throw new ArgumentException(nameof(settings.Version))
                };

                // If no version is available
                // we skip that package
                if (newVersion is null)
                {
                    continue;
                }

                // If the current version and new version are equal
                // we dont need to proceed here
                if (currentVersion == newVersion)
                {
                    continue;
                }

                // Interactive mode = User decides for each package
                // if it should be upgraded
                if (settings.Interactive)
                {
                    var question = string.Format(
                        "Upgrade {0} from {1} -> {2}",
                        packageName,
                        currentVersion,
                        newVersion
                    );

                    if (AnsiConsole.Confirm(question))
                    {
                        // Set the new package version
                        packageReference.SetAttributeValue("Version", newVersion);
                    }

                    continue;
                }

                // Set the new package version
                packageReference.SetAttributeValue("Version", newVersion);
            }

            if (settings.DryRun)
            {
                Console.WriteLine(csProjFile);
                return;
            }

            XmlWriterSettings wSettings = new XmlWriterSettings { Async = true, OmitXmlDeclaration = true };
            XmlWriter writer = XmlWriter.Create(pathToCsProjFile, wSettings);

            await csProjFile.SaveAsync(writer, CancellationToken.None);
            await writer.FlushAsync();
            writer.Close();
        }
    }
}

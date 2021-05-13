using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;

namespace PackCheck.Services
{
    public class CsProjFileService
    {
        public string GetPathToCsProjFile(string? pathToCsProjFile)
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
                    throw new CsProjFileNotFoundException(
                        $"File [white]{pathToCsProjFile}[/] does not exist in the current directory [white]{cwd}[/]"
                    );
                }

                return fullPath;
            }

            // No path was provided, we try to find a .csproj file
            var files = Directory.GetFiles(cwd, "*.csproj");

            return files.Length switch
            {
                0 => throw new CsProjFileNotFoundException(
                    $"Could not find a .csproj file in the current directory [white]{cwd}[/]"
                ),
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

            IEnumerable<XElement> items = csProjFile
                .Descendants("PackageReference")
                .Select(el => el);

            foreach (var item in items)
            {
                var packageName = item.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(packageName))
                {
                    var package = packages.SingleOrDefault(p => p.PackageName == packageName);
                    if (item.Attribute("Version") is not null && package is not null)
                    {
                        var version = settings.Version switch
                        {
                            "stable" => package.LatestStableVersion,
                            "latest" => package.LatestVersion,
                            _ => throw new ArgumentException(nameof(settings.Version))
                        };

                        item.SetAttributeValue("Version", version);
                    }
                }
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

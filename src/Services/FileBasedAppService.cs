using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Exceptions;
using Spectre.Console.Rendering;

namespace PackCheck.Services;

public static class FileBasedAppService
{
    private const string DirectivePrefix = "#:package";

    public static string GetPathToFileBasedAppFile(string pathToFbaFile)
    {
        var cwd = Directory.GetCurrentDirectory();

        // If pathToSlnFile is already a full path, it will not
        // be combined with the current directory.
        var fullPath = Path.Combine(cwd, pathToFbaFile);
        if (!File.Exists(fullPath))
        {
            throw new FileBasedAppFileException(
                $"File [white]{pathToFbaFile}[/] does not exist in the current directory [white]{cwd}[/]"
            );
        }

        return fullPath;
    }

    public static List<Package> GetPackagesDataFromFbaFile(string fbaFilePath)
    {
        var packages = new List<Package>();

        // Read all lines from the file
        var lines = File.ReadAllLines(fbaFilePath);
        foreach (var line in lines)
        {
            // Parse each line for package directives
            var package = ParsePackageDirective(line);
            if (package is not null)
            {
                packages.Add(package);
            }
        }

        return packages;
    }

    public static Package? ParsePackageDirective(string directive)
    {
        // Check if the string starts with #:package
        if (!directive.StartsWith(DirectivePrefix))
        {
            return null;
        }

        // Strip the prefix and store in variable
        string packageInfo = directive.Substring(DirectivePrefix.Length).Trim();

        // Parse the PackageName@Version format
        string[] parts = packageInfo.Split('@');
        if (parts.Length != 2)
        {
            return null; // Invalid format
        }

        string packageName = parts[0];
        string versionString = parts[1];

        // Validate package name is not empty
        if (string.IsNullOrEmpty(packageName))
        {
            return null;
        }

        // Parse version into NuGetVersion object
        if (!NuGetVersion.TryParse(versionString, out NuGetVersion? version))
        {
            return null; // Invalid version format
        }

        return new Package(packageName, version);
    }

    public static void UpgradePackageVersions(string fbaFilePath, List<Package> packages, bool dryRun)
    {
        var newLines = new List<string>();

        // Read all lines from the file
        var lines = File.ReadAllLines(fbaFilePath);
        foreach (var line in lines)
        {
            var currentLine = line.Trim();

            // Parse each line for package directives
            var package = ParsePackageDirective(currentLine);
            if (package is null)
            {
                newLines.Add(currentLine);
                continue; // Skip if not a package directive
            }

            // Find the package in the list
            var existingPackage = packages.Find(p => p.PackageName == package.PackageName);
            if (existingPackage is not null)
            {
                currentLine = currentLine.Replace(
                    $"{package.PackageName}@{package.CurrentVersion}",
                    $"{package.PackageName}@{existingPackage.NewVersion}"
                );
            }

            if (dryRun)
            {
                Console.WriteLine(currentLine);
            }

            newLines.Add(currentLine);
        }

        if (dryRun)
        {
            // If it's a dry run, we don't write back to the file
            return;
        }

        // Write the updated lines back to the file
        File.WriteAllLines(fbaFilePath, newLines);
    }
}

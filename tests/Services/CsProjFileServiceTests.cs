using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using PackCheck.Tests.Factories;
using Version = PackCheck.Data.Version;

namespace PackCheck.Tests.Services;

public class CsProjFileServiceTests
{
    [Fact]
    public void ThrowsWhenPathToCsProjFileIsProvidedButFileDoesNotExist()
    {
        var pathToFile = "does-not-exist.csproj";

        Action actual = () => CsProjFileService.GetPathToCsProjFile(pathToFile);

        Assert.Throws<CsProjFileException>(actual);
    }

    [Fact]
    public void CsProjFileExistsAtGivenPath()
    {
        TestHelper.LoadTestCsProjFile();
        var relativePath = "test.csproj";

        var fullPath = CsProjFileService.GetPathToCsProjFile(relativePath);

        Assert.EndsWith(relativePath, fullPath);
        TestHelper.DeleteTestCsProjFile();
    }

    [Fact]
    public void CsProjFileExistsWithoutGivenPath()
    {
        TestHelper.LoadTestCsProjFile();

        var fullPath = CsProjFileService.GetPathToCsProjFile();

        Assert.EndsWith("test.csproj", fullPath);
        TestHelper.DeleteTestCsProjFile();
    }

    [Fact]
    public async void AllPackagesGetUpdatedToLatestStableVersion()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Version = "stable",
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Version);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />", fileContent);
        TestHelper.DeleteTestCsProjFile();
    }

    [Fact]
    public async void OnePackageGetsUpdatedToLatestStableVersion()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var settings = new UpgradeSettings
        {
            PackageToUpgrade = "NuGet.Protocol",
            DryRun = false,
            Version = Version.Stable,
            Interactive = false
        };
        var packages = GeneratePackagesList()
            .Where(p => p.PackageName == settings.PackageToUpgrade)
            .ToList();

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Version);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.2.1\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.4.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.0\" />", fileContent);
        TestHelper.LoadTestCsProjFile();
    }

    [Fact]
    public async void AllPackagesGetUpdatedToLatestVersionIfAvailable()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Version = Version.Latest,
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Version);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"7.0.0-preview.123\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />", fileContent);
        TestHelper.LoadTestCsProjFile();
    }

    private List<Package> GeneratePackagesList()
    {
        // Packages und versions correspond to those in the test.csproj file
        return new()
        {
            PackageFactory.Create("NuGet.Protocol", "6.2.1", "6.3.0", "7.0.0-preview.123"),
            PackageFactory.Create("NuGet.Versioning", "6.2.1", "6.3.0"),
            PackageFactory.Create("Spectre.Cli.Extensions.DependencyInjection", "0.4.0", "0.5.0"),
            PackageFactory.Create("Spectre.Console", "0.44.0", "0.44.1"),
        };
    }
}

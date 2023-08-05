using System;
using System.Collections.Generic;
using System.IO;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using PackCheck.Tests.Factories;

namespace PackCheck.Tests.Services;

public class CentralPackageMgmtServiceTest
{
    [Fact]
    public void LoadsCpmFile()
    {
        TestHelper.LoadCpm();

        Assert.True(CentralPackageMgmtService.HasCentralPackageMgmt());

        TestHelper.DeleteCpm();
    }

    [Fact]
    public void ThrowsWhenPathToCpmFileIsProvidedButFileDoesNotExist()
    {
        var pathToFile = "does-not-exist.props";

        Action actual = () => CentralPackageMgmtService.GetPathToCpmFile(pathToFile);

        Assert.Throws<CpmFileException>(actual);
    }

    [Fact]
    public async void ReadsPackagesDataFromCpmFile()
    {
        TestHelper.LoadCpm();

        List<Package> packages = new();

        await CentralPackageMgmtService.GetPackagesDataFromCpmFileAsync(CentralPackageMgmtService.CpmFileName, packages);

        Assert.Equal(9, packages.Count);

        Assert.Equal("NuGet.Protocol", packages[0].PackageName);
        Assert.Equal("6.2.1", packages[0].CurrentVersion.ToString());
        Assert.Equal("stable", packages[0].UpgradeTo);

        Assert.Equal("Spectre.Console.Testing", packages[8].PackageName);
        Assert.Equal("0.44.0", packages[8].CurrentVersion.ToString());
        Assert.Equal("stable", packages[8].UpgradeTo);

        TestHelper.DeleteCpm();
    }

    [Fact]
    public async void AllPackagesGetUpdatedToLatestStableVersion()
    {
        TestHelper.LoadCpm();

        var pathToCsProjFile = CentralPackageMgmtService.CpmFileName;
        List<Package> packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Version = "stable",
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Version);

        await CentralPackageMgmtService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageVersion Include=\"NuGet.Protocol\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageVersion Include=\"NuGet.Versioning\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageVersion Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />", fileContent);
        Assert.Contains($"<PackageVersion Include=\"Spectre.Console\" Version=\"0.44.1\" />", fileContent);

        TestHelper.DeleteCpm();
    }

    private List<Package> GeneratePackagesList()
    {
        // Packages und versions correspond to those in the Directory.Packages.props file
        return new()
        {
            PackageFactory.Create("NuGet.Protocol", "6.2.1", "6.3.0", "7.0.0-preview.123"),
            PackageFactory.Create("NuGet.Versioning", "6.2.1", "6.3.0"),
            PackageFactory.Create("Spectre.Cli.Extensions.DependencyInjection", "0.4.0", "0.5.0"),
            PackageFactory.Create("Spectre.Console", "0.44.0", "0.44.1"),
        };
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Versioning;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests.Services;

public class CsProjFileServiceTests
{
    private readonly CsProjFileService _service;

    public CsProjFileServiceTests()
    {
        _service = new();
    }

    [Fact]
    public void ThrowsWhenPathToCsProjFileIsProvidedButFileDoesNotExist()
    {
        var pathToFile = "does-not-exist.csproj";

        Action actual = () => _service.GetPathToCsProjFile(pathToFile);

        Assert.Throws<CsProjFileNotFoundException>(actual);
    }

    [Fact]
    public void CsProjFileExistsAtGivenPath()
    {
        TestHelper.ResetTestCsProjFile();
        var relativePath = "test.csproj";

        var fullPath = _service.GetPathToCsProjFile(relativePath);

        Assert.EndsWith(relativePath, fullPath);
    }

    [Fact]
    public void CsProjFileExistsWithoutGivenPath()
    {
        TestHelper.ResetTestCsProjFile();

        var fullPath = _service.GetPathToCsProjFile();

        Assert.EndsWith("test.csproj", fullPath);
    }

    [Fact]
    public async void AllPackagesGetUpdatedToLatestStableVersion()
    {
        TestHelper.ResetTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Version = "stable",
            Interactive = false
        };

        await _service.UpgradePackageVersionsAsync(pathToCsProjFile, packages, settings);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />", fileContent);
    }

    [Fact]
    public async void OnePackageGetsUpdatedToLatestStableVersion()
    {
        TestHelper.ResetTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var settings = new UpgradeSettings
        {
            PackageToUpgrade = "NuGet.Protocol",
            DryRun = false,
            Version = "stable",
            Interactive = false
        };
        var packages = GeneratePackagesList()
            .Where(p => p.PackageName == settings.PackageToUpgrade)
            .ToList();

        await _service.UpgradePackageVersionsAsync(pathToCsProjFile, packages, settings);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.2.1\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.4.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.0\" />", fileContent);
    }

    [Fact]
    public async void AllPackagesGetUpdatedToLatestVersionIfAvailable()
    {
        TestHelper.ResetTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Version = "latest",
            Interactive = false
        };

        await _service.UpgradePackageVersionsAsync(pathToCsProjFile, packages, settings);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        Assert.Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"7.0.0-preview.123\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />", fileContent);
        Assert.Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />", fileContent);
    }

    private List<Package> GeneratePackagesList()
    {
        // Packages und versions correspond to those in the test.csproj file
        List<Package> packages = new()
        {
            new("NuGet.Protocol", NuGetVersion.Parse("6.2.1")) { LatestStableVersion = NuGetVersion.Parse("6.3.0"), LatestVersion = NuGetVersion.Parse("7.0.0-preview.123")},
            new("NuGet.Versioning", NuGetVersion.Parse("6.2.1")) { LatestStableVersion = NuGetVersion.Parse("6.3.0") },
            new("Spectre.Cli.Extensions.DependencyInjection", NuGetVersion.Parse("0.4.0")) { LatestStableVersion = NuGetVersion.Parse("0.5.0") },
            new("Spectre.Console", NuGetVersion.Parse("0.44.0")) { LatestStableVersion = NuGetVersion.Parse("0.44.1") },
        };

        return packages;
    }
}

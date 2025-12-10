using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using PackCheck.Tests.Factories;
using Target = PackCheck.Data.Target;

namespace PackCheck.Tests.Services;

public class CsProjFileServiceTests
{
    [Test]
    public async Task Throws_When_PathToCsProjFileIsProvidedButFileDoesNotExist()
    {
        var pathToFile = "does-not-exist.csproj";

        Action actual = () => CsProjFileService.GetPathToCsProjFile(pathToFile);

        await Assert.That(actual).ThrowsExactly<CsProjFileException>();
    }

    [Test]
    public async Task CsProjFileExistsAtGivenPath()
    {
        TestHelper.LoadTestCsProjFile();
        var relativePath = "test.csproj";

        var fullPath = CsProjFileService.GetPathToCsProjFile(relativePath);

        await Assert.That(fullPath).EndsWith(relativePath);
        TestHelper.DeleteTestCsProjFile();
    }

    [Test]
    public async Task CsProjFileExistsWithoutGivenPath()
    {
        TestHelper.LoadTestCsProjFile();

        var fullPath = CsProjFileService.GetPathToCsProjFile();

        await Assert.That(fullPath).EndsWith("test.csproj");
        TestHelper.DeleteTestCsProjFile();
    }

    [Test]
    public async Task AllPackagesGetUpdatedToLatestStableVersion()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Target = "stable",
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />");
        TestHelper.DeleteTestCsProjFile();
    }

    [Test]
    public async Task OnePackageGetsUpdatedToLatestStableVersion()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var settings = new UpgradeSettings
        {
            PackageToUpgrade = "NuGet.Protocol",
            DryRun = false,
            Target = Target.Stable,
            Interactive = false
        };
        var packages = GeneratePackagesList()
            .Where(p => p.PackageName == settings.PackageToUpgrade)
            .ToList();

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.2.1\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.4.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.0\" />");
        TestHelper.LoadTestCsProjFile();
    }

    [Test]
    public async Task AllPackagesGetUpdatedToLatestVersionIfAvailable()
    {
        TestHelper.LoadTestCsProjFile();
        var pathToCsProjFile = "test.csproj";
        var packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Target = Target.Latest,
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target);

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Protocol\" Version=\"7.0.0-preview.123\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"NuGet.Versioning\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />");
        await Assert.That(fileContent).Contains($"<PackageReference Include=\"Spectre.Console\" Version=\"0.44.1\" />");
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

using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using PackCheck.Tests.Factories;

namespace PackCheck.Tests.Services;

public class CentralPackageMgmtServiceTest
{
    [Test]
    public async Task LoadsCpmFile()
    {
        TestHelper.LoadCpm();

        await Assert.That(CentralPackageMgmtService.HasCentralPackageMgmt()).IsTrue();

        TestHelper.DeleteCpm();
    }

    [Test]
    public async Task ThrowsWhenPathToCpmFileIsProvidedButFileDoesNotExist()
    {
        var pathToFile = "does-not-exist.props";

        Action actual = () => CentralPackageMgmtService.GetPathToCpmFile(pathToFile);

        await Assert.That(actual).ThrowsExactly<CpmFileException>();
    }

    [Test]
    public async Task ReadsPackagesDataFromCpmFile()
    {
        TestHelper.LoadCpm();

        List<Package> packages = await CentralPackageMgmtService.GetPackagesDataFromCpmFileAsync(CentralPackageMgmtService.CpmFileName);

        await Assert.That(packages.Count).IsEqualTo(9);

        await Assert.That(packages[0].PackageName).IsEqualTo("NuGet.Protocol");
        await Assert.That(packages[0].CurrentVersion.ToString()).IsEqualTo("6.2.1");
        await Assert.That(packages[0].UpgradeTo).IsEqualTo("stable");
        await Assert.That(packages[8].PackageName).IsEqualTo("Spectre.Console.Testing");
        await Assert.That(packages[8].CurrentVersion.ToString()).IsEqualTo("0.44.0");
        await Assert.That(packages[8].UpgradeTo).IsEqualTo("stable");

        TestHelper.DeleteCpm();
    }

    [Test]
    public async Task AllPackagesGetUpdatedToLatestStableVersion()
    {
        TestHelper.LoadCpm();

        var pathToCsProjFile = CentralPackageMgmtService.CpmFileName;
        List<Package> packages = GeneratePackagesList();
        var settings = new UpgradeSettings
        {
            DryRun = false,
            Target = "stable",
            Interactive = false
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target);

        await CentralPackageMgmtService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, settings.DryRun);

        var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);

        await Assert.That(fileContent).Contains($"<PackageVersion Include=\"NuGet.Protocol\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageVersion Include=\"NuGet.Versioning\" Version=\"6.3.0\" />");
        await Assert.That(fileContent).Contains($"<PackageVersion Include=\"Spectre.Cli.Extensions.DependencyInjection\" Version=\"0.5.0\" />");
        await Assert.That(fileContent).Contains($"<PackageVersion Include=\"Spectre.Console\" Version=\"0.44.1\" />");

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

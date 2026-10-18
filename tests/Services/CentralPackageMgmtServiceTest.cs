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

    [Test]
    public async Task SkipsVersionsThatAreNotPlainVersions_And_HandlesDuplicates()
    {
        var pathToCpmFile = Path.Combine(Directory.GetCurrentDirectory(), "versions.props");
        await File.WriteAllTextAsync(pathToCpmFile, """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Pack.Property" Version="$(PackVersion)" />
                <PackageVersion Include="Pack.Range" Version="[1.0,2.0)" />
                <PackageVersion Include="Pack1" Version="8.0.0" Condition="'$(TargetFramework)' == 'net8.0'" />
                <PackageVersion Include="Pack1" Version="9.0.0" Condition="'$(TargetFramework)' == 'net9.0'" />
              </ItemGroup>
            </Project>
            """);

        try
        {
            var packages = await CentralPackageMgmtService.GetPackagesDataFromCpmFileAsync(pathToCpmFile);
            await Assert.That(packages.Select(p => p.CurrentVersion.ToString())).IsEquivalentTo(["8.0.0", "9.0.0"]);

            foreach (var package in packages)
            {
                package.LatestStableVersion = new("9.0.1");
            }

            var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, "stable");
            await CentralPackageMgmtService.UpgradePackageVersionsAsync(pathToCpmFile, preparedPackages, false);

            var fileContent = await File.ReadAllTextAsync(pathToCpmFile);
            await Assert.That(fileContent).Contains("<PackageVersion Include=\"Pack.Property\" Version=\"$(PackVersion)\" />");
            await Assert.That(fileContent).Contains("<PackageVersion Include=\"Pack.Range\" Version=\"[1.0,2.0)\" />");
            await Assert.That(fileContent).Contains("<PackageVersion Include=\"Pack1\" Version=\"9.0.1\" Condition=\"'$(TargetFramework)' == 'net8.0'\" />");
            await Assert.That(fileContent).Contains("<PackageVersion Include=\"Pack1\" Version=\"9.0.1\" Condition=\"'$(TargetFramework)' == 'net9.0'\" />");
        }
        finally
        {
            File.Delete(pathToCpmFile);
        }
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

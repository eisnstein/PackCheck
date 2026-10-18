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

    [Test]
    public async Task SkipsVersionsThatAreNotPlainVersions()
    {
        var pathToCsProjFile = Path.Combine(Directory.GetCurrentDirectory(), "versions.csproj");
        await File.WriteAllTextAsync(pathToCsProjFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Pack.Property" Version="$(PackVersion)" />
                <PackageReference Include="Pack.Range" Version="[1.0,2.0)" />
                <PackageReference Include="Pack.Floating" Version="1.*" />
                <PackageReference Include="Pack.Plain" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);

        try
        {
            var packages = await CsProjFileService.GetPackagesDataFromCsProjFileAsync(pathToCsProjFile);

            await Assert.That(packages.Select(p => p.PackageName)).IsEquivalentTo(["Pack.Plain"]);

            packages[0].LatestStableVersion = new("1.1.0");
            var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);
            await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, false);

            var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack.Plain\" Version=\"1.1.0\" />");
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack.Property\" Version=\"$(PackVersion)\" />");
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack.Range\" Version=\"[1.0,2.0)\" />");
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack.Floating\" Version=\"1.*\" />");
        }
        finally
        {
            File.Delete(pathToCsProjFile);
        }
    }

    [Test]
    public async Task UpgradesOnlySelectedEntry_When_PackageIsReferencedMoreThanOnce()
    {
        var pathToCsProjFile = Path.Combine(Directory.GetCurrentDirectory(), "duplicates.csproj");
        await File.WriteAllTextAsync(pathToCsProjFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                <PackageReference Include="Pack1" Version="8.0.0" />
              </ItemGroup>
              <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                <PackageReference Include="Pack1" Version="9.0.0" />
              </ItemGroup>
            </Project>
            """);

        try
        {
            var packages = await CsProjFileService.GetPackagesDataFromCsProjFileAsync(pathToCsProjFile);
            await Assert.That(packages).Count().IsEqualTo(2);

            foreach (var package in packages)
            {
                package.LatestStableVersion = new("9.0.1");
            }

            // Like in interactive mode: only the net9.0 entry is selected
            var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable)
                .Where(p => p.CurrentVersion.Major == 9)
                .ToList();
            await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, preparedPackages, false);

            var fileContent = await File.ReadAllTextAsync(pathToCsProjFile);
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack1\" Version=\"8.0.0\" />");
            await Assert.That(fileContent).Contains("<PackageReference Include=\"Pack1\" Version=\"9.0.1\" />");
        }
        finally
        {
            File.Delete(pathToCsProjFile);
        }
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

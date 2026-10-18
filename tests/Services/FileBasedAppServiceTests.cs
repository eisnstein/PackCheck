using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class FileBasedAppServiceTests
{
    [Test]
    public async Task LoadsFbaFile()
    {
        TestHelper.LoadFba();

        var path = FileBasedAppService.GetPathToFileBasedAppFile("app.cs");

        await Assert.That(path).EndsWith("app.cs");

        TestHelper.DeleteFba();
    }

    [Test]
    public async Task ReturnsNullWhenInputDoesNotStartWithPackageDirective()
    {
        // Arrange
        var input = "some random text";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenInputIsEmpty()
    {
        // Arrange
        var input = "";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenInputIsOnlyPackageDirective()
    {
        // Arrange
        var input = "#:package";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenPackageInfoIsMissingAtSymbol()
    {
        // Arrange
        var input = "#:package MyPackage1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenPackageInfoHasMultipleAtSymbols()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0@extra";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenVersionStringIsInvalid()
    {
        // Arrange
        var input = "#:package MyPackage@invalid-version";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsPackageWhenValidPackageDirectiveWithStableVersion()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("MyPackage");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("1.0.0");
    }

    [Test]
    public async Task ReturnsPackageWhenValidPackageDirectiveWithPrereleaseVersion()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0-beta1";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("MyPackage");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("1.0.0-beta1");
    }

    [Test]
    public async Task ReturnsPackageWhenValidPackageDirectiveWithComplexVersion()
    {
        // Arrange
        var input = "#:package NuGet.Protocol@6.2.1-preview.1.123.45";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("NuGet.Protocol");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("6.2.1-preview.1.123.45");
    }

    [Test]
    public async Task HandlesWhitespaceAroundPackageInfo()
    {
        // Arrange
        var input = "#:package   MyPackage@1.0.0   ";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("MyPackage");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("1.0.0");
    }

    [Test]
    public async Task ReturnsPackageWithPackageNameContainingDots()
    {
        // Arrange
        var input = "#:package Microsoft.Extensions.Logging@7.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("Microsoft.Extensions.Logging");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("7.0.0");
    }

    [Test]
    public async Task ReturnsNullWhenPackageNameIsEmpty()
    {
        // Arrange
        var input = "#:package @1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsNullWhenVersionIsEmpty()
    {
        // Arrange
        var input = "#:package MyPackage@";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ReturnsPackageWhenVersionHasFourParts()
    {
        // Arrange
        var input = "#:package MyPackage@1.2.3.4";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.PackageName).IsEqualTo("MyPackage");
        await Assert.That(result.CurrentVersion.ToString()).IsEqualTo("1.2.3.4");
    }

    [Test]
    public async Task GetsPackagesDataFromFbaFile()
    {
        TestHelper.LoadFba();

        var fbaFilePath = FileBasedAppService.GetPathToFileBasedAppFile("app.cs");
        Assert.NotNull(fbaFilePath);

        var packages = FileBasedAppService.GetPackagesDataFromFbaFile(fbaFilePath);

        await Assert.That(packages).IsNotEmpty();
        await Assert.That(packages).Count().IsEqualTo(1);
        await Assert.That(packages[0].PackageName).IsEqualTo("Humanizer");
        await Assert.That(packages[0].CurrentVersion.ToString()).IsEqualTo("2.14.1");

        TestHelper.DeleteFba();
    }

    [Test]
    public async Task WritesNewVersionToFile()
    {
        TestHelper.LoadFba();

        var fbaFilePath = FileBasedAppService.GetPathToFileBasedAppFile("app.cs");
        Assert.NotNull(fbaFilePath);

        var packages = FileBasedAppService.GetPackagesDataFromFbaFile(fbaFilePath);
        var latestStableVerion = new NuGetVersion("2.15.0");
        packages[0].LatestStableVersion = latestStableVerion;
        PackagesService.PreparePackagesForUpgrade(packages, "stable");

        FileBasedAppService.UpgradePackageVersions(fbaFilePath, packages, false);

        var updatedPackages = FileBasedAppService.GetPackagesDataFromFbaFile(fbaFilePath);

        await Assert.That(updatedPackages).IsNotEmpty();
        await Assert.That(updatedPackages).Count().IsEqualTo(1);
        await Assert.That(updatedPackages[0].PackageName).IsEqualTo("Humanizer");
        await Assert.That(updatedPackages[0].CurrentVersion.ToString()).IsEqualTo("2.15.0");

        TestHelper.DeleteFba();
    }

    [Test]
    [MatrixDataSource]
    public async Task PreservesIndentationLineEndingsAndBomWhenUpgrading(
        [Matrix("\n", "\r\n")] string newLine,
        [Matrix(true, false)] bool withBom)
    {
        byte[] Encode(string content) => withBom
            ? [.. System.Text.Encoding.UTF8.GetPreamble(), .. System.Text.Encoding.UTF8.GetBytes(content)]
            : System.Text.Encoding.UTF8.GetBytes(content);

        string[] lines =
        [
            "#:package Humanizer@2.14.1",
            "",
            "if (args.Length > 0)",
            "{",
            "    Console.WriteLine(args[0]);",
            "}",
            "",
        ];
        var fbaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "indented.cs");
        File.WriteAllBytes(fbaFilePath, Encode(string.Join(newLine, lines)));

        try
        {
            var packages = FileBasedAppService.GetPackagesDataFromFbaFile(fbaFilePath);
            packages[0].LatestStableVersion = new NuGetVersion("2.15.0");
            PackagesService.PreparePackagesForUpgrade(packages, "stable");

            FileBasedAppService.UpgradePackageVersions(fbaFilePath, packages, false);

            lines[0] = "#:package Humanizer@2.15.0";
            // Compare bytes, File.ReadAllText would hide a missing BOM
            await Assert.That(File.ReadAllBytes(fbaFilePath)).IsEquivalentTo(Encode(string.Join(newLine, lines)));
        }
        finally
        {
            File.Delete(fbaFilePath);
        }
    }
}

using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class FileBasedAppServiceTests
{
    [Fact]
    public void LoadsFbaFile()
    {
        TestHelper.LoadFba();

        var path = FileBasedAppService.GetPathToFileBasedAppFile("app.cs");

        Assert.EndsWith("app.cs", path);

        TestHelper.DeleteFba();
    }

    [Fact]
    public void ReturnsNullWhenInputDoesNotStartWithPackageDirective()
    {
        // Arrange
        var input = "some random text";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenInputIsEmpty()
    {
        // Arrange
        var input = "";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenInputIsOnlyPackageDirective()
    {
        // Arrange
        var input = "#:package";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenPackageInfoIsMissingAtSymbol()
    {
        // Arrange
        var input = "#:package MyPackage1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenPackageInfoHasMultipleAtSymbols()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0@extra";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenVersionStringIsInvalid()
    {
        // Arrange
        var input = "#:package MyPackage@invalid-version";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsPackageWhenValidPackageDirectiveWithStableVersion()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyPackage", result.PackageName);
        Assert.Equal("1.0.0", result.CurrentVersion.ToString());
    }

    [Fact]
    public void ReturnsPackageWhenValidPackageDirectiveWithPrereleaseVersion()
    {
        // Arrange
        var input = "#:package MyPackage@1.0.0-beta1";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyPackage", result.PackageName);
        Assert.Equal("1.0.0-beta1", result.CurrentVersion.ToString());
    }

    [Fact]
    public void ReturnsPackageWhenValidPackageDirectiveWithComplexVersion()
    {
        // Arrange
        var input = "#:package NuGet.Protocol@6.2.1-preview.1.123.45";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NuGet.Protocol", result.PackageName);
        Assert.Equal("6.2.1-preview.1.123.45", result.CurrentVersion.ToString());
    }

    [Fact]
    public void HandlesWhitespaceAroundPackageInfo()
    {
        // Arrange
        var input = "#:package   MyPackage@1.0.0   ";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyPackage", result.PackageName);
        Assert.Equal("1.0.0", result.CurrentVersion.ToString());
    }

    [Fact]
    public void ReturnsPackageWithPackageNameContainingDots()
    {
        // Arrange
        var input = "#:package Microsoft.Extensions.Logging@7.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Microsoft.Extensions.Logging", result.PackageName);
        Assert.Equal("7.0.0", result.CurrentVersion.ToString());
    }

    [Fact]
    public void ReturnsNullWhenPackageNameIsEmpty()
    {
        // Arrange
        var input = "#:package @1.0.0";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNullWhenVersionIsEmpty()
    {
        // Arrange
        var input = "#:package MyPackage@";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReturnsPackageWhenVersionHasFourParts()
    {
        // Arrange
        var input = "#:package MyPackage@1.2.3.4";

        // Act
        var result = FileBasedAppService.ParsePackageDirective(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyPackage", result.PackageName);
        Assert.Equal("1.2.3.4", result.CurrentVersion.ToString());
    }

    [Fact]
    public void GetsPackagesDataFromFbaFile()
    {
        TestHelper.LoadFba();

        var fbaFilePath = FileBasedAppService.GetPathToFileBasedAppFile("app.cs");
        Assert.NotNull(fbaFilePath);

        var packages = FileBasedAppService.GetPackagesDataFromFbaFile(fbaFilePath);

        Assert.NotEmpty(packages);
        Assert.Single(packages);
        Assert.Equal("Humanizer", packages[0].PackageName);
        Assert.Equal("2.14.1", packages[0].CurrentVersion.ToString());

        TestHelper.DeleteFba();
    }

    [Fact]
    public void WritesNewVersionToFile()
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
        Assert.NotEmpty(updatedPackages);
        Assert.Single(updatedPackages);
        Assert.Equal("Humanizer", updatedPackages[0].PackageName);
        Assert.Equal("2.15.0", updatedPackages[0].CurrentVersion.ToString());

        TestHelper.DeleteFba();
    }
}

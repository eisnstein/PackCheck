using System.Collections.Generic;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Services;
using PackCheck.Tests.Factories;

namespace PackCheck.Tests.Services;

public class PackagesServiceTest
{
    [Fact]
    public void Returns_UnchangedPackagesList_When_ConfigIsNull()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
        };

        var newPackages = PackagesService.ApplySettings(packages, null);

        Assert.Equal(packages, newPackages);
    }

    [Fact]
    public void Returns_ChangedPackagesList_When_FilterIsSet()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
        };
        CheckSettings settings = new()
        {
            Filter = ["Pack1"]
        };

        var newPackages = PackagesService.ApplySettings(packages, settings);

        Assert.Single(newPackages);
        Assert.Equal("Pack1", newPackages[0].PackageName);
    }

    [Fact]
    public void Returns_ChangedPackagesList_When_ExcludeIsSet()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
        };
        CheckSettings settings = new()
        {
            Exclude = ["Pack1"]
        };

        var newPackages = PackagesService.ApplySettings(packages, settings);

        Assert.Single(newPackages);
        Assert.Equal("Pack2", newPackages[0].PackageName);
    }

    [Fact]
    public void Returns_EmptyPackagesList_When_FilterAndExcludeIsSet()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
        };
        CheckSettings settings = new()
        {
            Filter = ["Pack1"],
            Exclude = ["Pack1"]
        };

        var newPackages = PackagesService.ApplySettings(packages, settings);

        Assert.Empty(newPackages);
    }

    [Fact]
    public void Returns_EmptyList_When_NoPackageNeedsUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.0"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.0"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);

        Assert.Empty(preparedPackages);
    }

    [Fact]
    public void Returns_AllPackagesForStableUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.2", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.3.0"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.1"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.1"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);

        Assert.Equal(4, preparedPackages.Count);
        Assert.Equal("6.2.2", preparedPackages[0]!.NewVersion!.ToString());
        Assert.Equal("6.3.0", preparedPackages[1]!.NewVersion!.ToString());
        Assert.Equal("0.4.1", preparedPackages[2]!.NewVersion!.ToString());
        Assert.Equal("0.44.1", preparedPackages[3]!.NewVersion!.ToString());

        foreach (var package in preparedPackages)
        {
            Assert.Equal(Target.Stable, package.UpgradeTo);
        }
    }

    [Fact]
    public void Returns_OnePackageForLatestUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.0"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.0"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Latest);

        Assert.Single(preparedPackages);
        Assert.Equal("6.2.2", preparedPackages[0]!.NewVersion!.ToString());
        Assert.Equal("Pack1", preparedPackages[0]!.PackageName);
        Assert.Equal(Target.Latest, preparedPackages[0]!.UpgradeTo);
    }

    [Fact]
    public void Returns_AllPackagesForLatestUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.2", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.3.0", "7.0.0-preview1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.1", "0.4.1"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.1", "0.5.0-rc.1"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Latest);

        Assert.Equal(4, preparedPackages.Count);
        Assert.Equal("6.2.2", preparedPackages[0]!.NewVersion!.ToString());
        Assert.Equal("7.0.0-preview1", preparedPackages[1]!.NewVersion!.ToString());
        Assert.Equal("0.4.1", preparedPackages[2]!.NewVersion!.ToString());
        Assert.Equal("0.5.0-rc.1", preparedPackages[3]!.NewVersion!.ToString());

        foreach (var package in preparedPackages)
        {
            Assert.Equal(Target.Latest, package.UpgradeTo);
        }
    }
}

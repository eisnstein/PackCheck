using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Services;
using PackCheck.Tests.Factories;
using Target = PackCheck.Data.Target;

namespace PackCheck.Tests.Services;

public class PackagesServiceTest
{
    [Test]
    public async Task Returns_UnchangedPackagesList_When_ConfigIsNull()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
        };

        var newPackages = PackagesService.ApplySettings(packages, null);

        await Assert.That(newPackages).IsEqualTo(packages);
    }

    [Test]
    public async Task Returns_ChangedPackagesList_When_FilterIsSet()
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

        await Assert.That(newPackages).Count().IsEqualTo(1);
        await Assert.That(newPackages[0].PackageName).IsEqualTo("Pack1");
    }

    [Test]
    public async Task Returns_ChangedPackagesList_When_ExcludeIsSet()
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

        await Assert.That(newPackages).Count().IsEqualTo(1);
        await Assert.That(newPackages[0].PackageName).IsEqualTo("Pack2");
    }

    [Test]
    public async Task Returns_EmptyPackagesList_When_FilterAndExcludeIsSet()
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

        await Assert.That(newPackages).IsEmpty();
    }

    [Test]
    public async Task Returns_EmptyList_When_NoPackageNeedsUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.0"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.0"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);

        await Assert.That(preparedPackages).IsEmpty();
    }

    [Test]
    public async Task Returns_AllPackagesForStableUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.2", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.3.0"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.1"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.1"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);

        await Assert.That(preparedPackages).Count().IsEqualTo(4);
        await Assert.That(preparedPackages[0]!.NewVersion!.ToString()).IsEqualTo("6.2.2");
        await Assert.That(preparedPackages[1]!.NewVersion!.ToString()).IsEqualTo("6.3.0");
        await Assert.That(preparedPackages[2]!.NewVersion!.ToString()).IsEqualTo("0.4.1");
        await Assert.That(preparedPackages[3]!.NewVersion!.ToString()).IsEqualTo("0.44.1");
        foreach (var package in preparedPackages)
        {
            await Assert.That(package.UpgradeTo).IsEqualTo(Target.Stable);
        }
    }

    [Test]
    public async Task Returns_OnePackageForLatestUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.1", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.2.1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.0"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.0"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Latest);

        await Assert.That(preparedPackages).Count().IsEqualTo(1);
        await Assert.That(preparedPackages[0]!.NewVersion!.ToString()).IsEqualTo("6.2.2");
        await Assert.That(preparedPackages[0]!.PackageName).IsEqualTo("Pack1");
        await Assert.That(preparedPackages[0]!.UpgradeTo).IsEqualTo(Target.Latest);
    }

    [Test]
    public async Task Returns_AllPackagesForLatestUpgrade()
    {
        List<Package> packages = new()
        {
            PackageFactory.Create("Pack1", "6.2.1", "6.2.2", "6.2.2"),
            PackageFactory.Create("Pack2", "6.2.1", "6.3.0", "7.0.0-preview1"),
            PackageFactory.Create("Pack3", "0.4.0", "0.4.1", "0.4.1"),
            PackageFactory.Create("Pack4", "0.44.0", "0.44.1", "0.5.0-rc.1"),
        };

        var preparedPackages = PackagesService.PreparePackagesForUpgrade(packages, Target.Latest);

        await Assert.That(preparedPackages).Count().IsEqualTo(4);
        await Assert.That(preparedPackages[0]!.NewVersion!.ToString()).IsEqualTo("6.2.2");
        await Assert.That(preparedPackages[1]!.NewVersion!.ToString()).IsEqualTo("7.0.0-preview1");
        await Assert.That(preparedPackages[2]!.NewVersion!.ToString()).IsEqualTo("0.4.1");
        await Assert.That(preparedPackages[3]!.NewVersion!.ToString()).IsEqualTo("0.5.0-rc.1");
        foreach (var package in preparedPackages)
        {
            await Assert.That(package.UpgradeTo).IsEqualTo(Target.Latest);
        }
    }
}

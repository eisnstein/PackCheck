using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class NuGetVersionServiceTests
{
    [Test]
    public async Task GetsLatestVersion()
    {
        var versions = new List<NuGetVersion>
        {
            new ("1.0.0"),
            new ("1.1.0"),
            new ("1.1.1"),
            new ("1.1.1-preview.1.21102.12"),
            new ("2.0.0"),
            new ("2.1.9-preview.2"),
            new ("2.1.9-rc.1.2334.2"),
            new ("2.1.9"),
            new ("2.2.0"),
            new ("3.0.1"),
            new ("4.0.1"),
            new ("4.0.2"),
        };

        var latestVersion = NuGetVersionService.GetLatestVersion(versions);

        await Assert.That(latestVersion.ToString()).IsEqualTo("4.0.2");
    }

    [Test]
    public async Task GetsLatestVersionWhenLatestVersionIsPrerelease()
    {
        var versions = new List<NuGetVersion>
        {
            new ("1.0.0"),
            new ("1.1.0"),
            new ("1.1.1"),
            new ("1.1.1-preview.1.21102.12"),
            new ("2.0.0"),
            new ("2.1.9-preview.2"),
            new ("2.1.9-rc.1.2334.2"),
            new ("2.1.9"),
            new ("2.2.0"),
            new ("3.0.1"),
            new ("4.0.1"),
            new ("4.0.2"),
            new ("4.0.2-preview.1.123.4"),
        };

        var latestVersion = NuGetVersionService.GetLatestVersion(versions);

        await Assert.That(latestVersion.ToString()).IsEqualTo("4.0.2-preview.1.123.4");
    }

    [Test]
    public async Task GetsLatestStableVersion()
    {
        var versions = new List<NuGetVersion>
        {
            new ("1.0.0"),
            new ("1.1.0"),
            new ("1.1.1"),
            new ("1.1.1-preview.1.21102.12"),
            new ("2.0.0"),
            new ("2.1.9-preview.2"),
            new ("2.1.9-rc.1.2334.2"),
            new ("2.1.9"),
            new ("2.2.0"),
            new ("3.0.1"),
            new ("4.0.1"),
            new ("4.0.2"),
        };

        var stableVersion1 = NuGetVersionService.GetLatestStableVersion(versions);
        var latestVersion1 = NuGetVersionService.GetLatestVersion(versions);
        var stableVersion2 = NuGetVersionService.GetLatestStableVersion(versions);
        var latestVersion2 = NuGetVersionService.GetLatestVersion(versions);

        await Assert.That(stableVersion1).IsNotNull();
        await Assert.That(stableVersion1.ToString()).IsEqualTo("4.0.2");
        await Assert.That(latestVersion1.ToString()).IsEqualTo("4.0.2");
        await Assert.That(stableVersion2).IsNotNull();
        await Assert.That(stableVersion2.ToString()).IsEqualTo("4.0.2");
        await Assert.That(latestVersion2.ToString()).IsEqualTo("4.0.2");
    }

    [Test]
    public async Task GetsLatestStableVersionWhenPackageOnLatestPrereleaseVersion()
    {
        var versions = new List<NuGetVersion>
        {
            new ("1.0.0"),
            new ("1.1.0"),
            new ("1.1.1"),
            new ("1.1.1-preview.1.21102.12"),
            new ("2.0.0"),
            new ("2.1.9-preview.2"),
            new ("2.1.9-rc.1.2334.2"),
            new ("2.1.9"),
            new ("2.2.0"),
            new ("3.0.1"),
            new ("4.0.1"),
            new ("4.0.2"),
            new ("5.0.0-preview.1"),
        };

        var latestStableVersion = NuGetVersionService.GetLatestStableVersion(versions);

        await Assert.That(latestStableVersion).IsNotNull();
        await Assert.That(latestStableVersion.ToString()).IsEqualTo("4.0.2");
    }

    [Test]
    public async Task GetLatestVersionWhenOnlyPrereleaseVersionsAvailable()
    {
        var versions = new List<NuGetVersion>
        {
            new ("4.0.0-preview"),
            new ("4.0.0-preview2"),
            new ("4.0.0-preview3"),
        };

        var latestStableVersion = NuGetVersionService.GetLatestStableVersion(versions);
        var latestVersion = NuGetVersionService.GetLatestVersion(versions);

        await Assert.That(latestStableVersion).IsNull();
        await Assert.That(latestVersion.ToString()).IsEqualTo("4.0.0-preview3");
    }
}

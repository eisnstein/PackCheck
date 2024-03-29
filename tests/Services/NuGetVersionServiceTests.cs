using System.Collections.Generic;
using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class NuGetVersionServiceTests
{
    [Fact]
    public void GetsLatestVersion()
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

        Assert.Equal("4.0.2", latestVersion.ToString());
    }

    [Fact]
    public void GetsLatestVersionWhenLatestVersionIsPrerelease()
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

        Assert.Equal("4.0.2-preview.1.123.4", latestVersion.ToString());
    }

    [Fact]
    public void GetsLatestStableVersion()
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

        Assert.NotNull(stableVersion1);
        Assert.Equal("4.0.2", stableVersion1.ToString());
        Assert.Equal("4.0.2", latestVersion1.ToString());
        Assert.NotNull(stableVersion2);
        Assert.Equal("4.0.2", stableVersion2.ToString());
        Assert.Equal("4.0.2", latestVersion2.ToString());
    }

    [Fact]
    public void GetsLatestStableVersionWhenPackageOnLatestPrereleaseVersion()
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

        Assert.NotNull(latestStableVersion);
        Assert.Equal("4.0.2", latestStableVersion.ToString());
    }

    [Fact]
    public void GetLatestVersionWhenOnlyPrereleaseVersionsAvailable()
    {
        var versions = new List<NuGetVersion>
        {
            new ("4.0.0-preview"),
            new ("4.0.0-preview2"),
            new ("4.0.0-preview3"),
        };

        var latestStableVersion = NuGetVersionService.GetLatestStableVersion(versions);
        var latestVersion = NuGetVersionService.GetLatestVersion(versions);

        Assert.Null(latestStableVersion);
        Assert.Equal("4.0.0-preview3", latestVersion.ToString());
    }
}

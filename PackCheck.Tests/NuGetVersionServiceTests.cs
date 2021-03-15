using System.Collections.Generic;
using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests
{
    public class NuGetVersionServiceTests
    {
        [Fact]
        public void GetsLatestVersion()
        {
            var service = new NuGetVersionService();
            var versions = new List<NuGetVersion>
            {
                new NuGetVersion("1.0.0"),
                new NuGetVersion("1.1.0"),
                new NuGetVersion("1.1.1"),
                new NuGetVersion("1.1.1-preview.1.21102.12"),
                new NuGetVersion("2.0.0"),
                new NuGetVersion("2.1.9-preview.2"),
                new NuGetVersion("2.1.9-rc.1.2334.2"),
                new NuGetVersion("2.1.9"),
                new NuGetVersion("2.2.0"),
                new NuGetVersion("3.0.1"),
                new NuGetVersion("4.0.1"),
                new NuGetVersion("4.0.2"),
            };
            var package = new Package("some.package.name", new NuGetVersion("2.1.9"));

            var latestVersion = service.GetLatestVersion(package, versions);

            Assert.Equal("4.0.2", latestVersion.ToString());
        }

        [Fact]
        public void GetsLatestVersionWhenLatestVersionIsPrelease()
        {
            var service = new NuGetVersionService();
            var versions = new List<NuGetVersion>
            {
                new NuGetVersion("1.0.0"),
                new NuGetVersion("1.1.0"),
                new NuGetVersion("1.1.1"),
                new NuGetVersion("1.1.1-preview.1.21102.12"),
                new NuGetVersion("2.0.0"),
                new NuGetVersion("2.1.9-preview.2"),
                new NuGetVersion("2.1.9-rc.1.2334.2"),
                new NuGetVersion("2.1.9"),
                new NuGetVersion("2.2.0"),
                new NuGetVersion("3.0.1"),
                new NuGetVersion("4.0.1"),
                new NuGetVersion("4.0.2"),
                new NuGetVersion("4.0.2-preview.1.123.4"),
            };
            var package = new Package("some.package.name", new NuGetVersion("2.1.9"));

            var latestVersion = service.GetLatestVersion(package, versions);

            Assert.Equal("4.0.2-preview.1.123.4", latestVersion.ToString());
        }

        [Fact]
        public void GetsLatestStableVersion()
        {
            var service = new NuGetVersionService();
            var versions = new List<NuGetVersion>
            {
                new NuGetVersion("1.0.0"),
                new NuGetVersion("1.1.0"),
                new NuGetVersion("1.1.1"),
                new NuGetVersion("1.1.1-preview.1.21102.12"),
                new NuGetVersion("2.0.0"),
                new NuGetVersion("2.1.9-preview.2"),
                new NuGetVersion("2.1.9-rc.1.2334.2"),
                new NuGetVersion("2.1.9"),
                new NuGetVersion("2.2.0"),
                new NuGetVersion("3.0.1"),
                new NuGetVersion("4.0.1"),
                new NuGetVersion("4.0.2"),
            };
            var package1 = new Package("some.package.name", new NuGetVersion("2.1.9"));
            var package2 = new Package("some.package.name", new NuGetVersion("1.0.0"));

            var stableVersion1 = service.GetLatestStableVersion(package1, versions);
            var latestVersion1 = service.GetLatestVersion(package1, versions);
            var stableVersion2 = service.GetLatestStableVersion(package2, versions);
            var latestVersion2 = service.GetLatestVersion(package2, versions);

            Assert.Equal("4.0.2", stableVersion1.ToString());
            Assert.Equal("4.0.2", latestVersion1.ToString());
            Assert.Equal("4.0.2", stableVersion2.ToString());
            Assert.Equal("4.0.2", latestVersion2.ToString());
        }

        [Fact]
        public void GetsLatestStableVersionWhenPackageOnLatestPreleaseVersion()
        {
            var service = new NuGetVersionService();
            var versions = new List<NuGetVersion>
            {
                new NuGetVersion("1.0.0"),
                new NuGetVersion("1.1.0"),
                new NuGetVersion("1.1.1"),
                new NuGetVersion("1.1.1-preview.1.21102.12"),
                new NuGetVersion("2.0.0"),
                new NuGetVersion("2.1.9-preview.2"),
                new NuGetVersion("2.1.9-rc.1.2334.2"),
                new NuGetVersion("2.1.9"),
                new NuGetVersion("2.2.0"),
                new NuGetVersion("3.0.1"),
                new NuGetVersion("4.0.1"),
                new NuGetVersion("4.0.2"),
                new NuGetVersion("5.0.0-preview.1"),
            };
            var package1 = new Package("some.package.name", new NuGetVersion("5.0.0-preview.1"));

            var latestStableVersion1 = service.GetLatestStableVersion(package1, versions);

            Assert.Equal("4.0.2", latestStableVersion1.ToString());
        }
    }
}

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

            var latestVersion1 = service.GetLatestStableVersion(package1, versions);
            var latestVersion2 = service.GetLatestStableVersion(package2, versions);

            Assert.Equal("2.2.0", latestVersion1.ToString());
            Assert.Equal("1.1.1", latestVersion2.ToString());
        }
    }
}

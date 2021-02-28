using System.Collections.Generic;
using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests
{
    public class NuGetPackagesServiceTests
    {
        [Fact]
        public void GetLatestVersion_GetLatestVersionOfAllVersions()
        {
            var service = new NuGetPackagesService();
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
            var package = new Package("some.package.name", new("2.1.9"));

            var latestVersion = service.GetLatestVersion(package, versions);

            Assert.NotNull(latestVersion);
            Assert.Equal("4.0.2", latestVersion.ToString());
        }
    }
}

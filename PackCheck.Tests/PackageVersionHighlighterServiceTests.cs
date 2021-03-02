using PackCheck.Data;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests
{
    public class PackageVersionHighlighterServiceTests
    {
        [Fact]
        public void HighlightsNothingWhenVersionAreTheSame()
        {
            var service = new PackageVersionHighlighterService();
            var package = new Package("test.package", new("4.0.4"));
            package.LatestStableVersion = new("4.0.4");
            package.LatestVersion = new("4.0.4");

            var highlightedStableVersion = service.HighlightLatestStableVersion(package);
            var highlightedLatestVersion = service.HighlightLatestVersion(package);

            Assert.Equal("4.0.4", highlightedStableVersion);
            Assert.Equal("4.0.4", highlightedLatestVersion);
        }

        [Fact]
        public void HighlightsThePatchPart()
        {
            var service = new PackageVersionHighlighterService();
            var package = new Package("test.package", new("1.0.1"));
            package.LatestStableVersion = new("1.0.2");
            package.LatestVersion = new("1.0.2");

            var highlightedStableVersion = service.HighlightLatestStableVersion(package);
            var highlightedLatestVersion = service.HighlightLatestVersion(package);

            Assert.Equal("1.0.[green]2[/]", highlightedStableVersion);
            Assert.Equal("1.0.[green]2[/]", highlightedLatestVersion);
        }

        [Fact]
        public void HighlightsThePrereleasePart()
        {
            var service = new PackageVersionHighlighterService();
            var package = new Package("test.package", new("1.0.1-rc1-final"));
            package.LatestStableVersion = new("1.0.1-rc2-final");
            package.LatestVersion = new("1.0.1-rc2-final");

            var highlightedStableVersion = service.HighlightLatestStableVersion(package);
            var highlightedLatestVersion = service.HighlightLatestVersion(package);

            Assert.Equal("1.0.1[green]-rc2-final[/]", highlightedStableVersion);
            Assert.Equal("1.0.1[green]-rc2-final[/]", highlightedLatestVersion);
        }

        [Fact]
        public void HighlightsTheMinorPart()
        {
            var service = new PackageVersionHighlighterService();
            var package = new Package("test.package", new("1.0.1"));
            package.LatestStableVersion = new("1.1.0");
            package.LatestVersion = new("1.1.0");

            var highlightedStableVersion = service.HighlightLatestStableVersion(package);
            var highlightedVersion = service.HighlightLatestVersion(package);

            Assert.Equal("1.[lime]1.0[/]", highlightedStableVersion);
            Assert.Equal("1.[lime]1.0[/]", highlightedVersion);
        }

        [Fact]
        public void HighlightsTheMajorPart()
        {
            var service = new PackageVersionHighlighterService();
            var package = new Package("test.package", new("1.0.1"));
            package.LatestStableVersion = new("2.0.0");
            package.LatestVersion = new("2.0.0");

            var highlightedStableVersion = service.HighlightLatestStableVersion(package);
            var highlightedLatestVersion = service.HighlightLatestVersion(package);

            Assert.Equal("[red]2.0.0[/]", highlightedStableVersion);
            Assert.Equal("[red]2.0.0[/]", highlightedLatestVersion);
        }
    }
}

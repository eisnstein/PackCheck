using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class PackageVersionHighlighterServiceTests
{
    [Fact]
    public void HighlightsNothingWhenVersionAreTheSame()
    {
        var package = new Package("test.package", new("4.0.4"));
        package.LatestStableVersion = new("4.0.4");
        package.LatestVersion = new("4.0.4");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("4.0.4", highlightedStableVersion);
        Assert.Equal("4.0.4", highlightedLatestVersion);
    }

    [Fact]
    public void HighlightsGreenWhenOlderVersionIsPrelease()
    {
        var package = new Package("test.package", new("5.0.0-rc.2.20475.5"));
        package.LatestStableVersion = new("5.0.0");
        package.LatestVersion = new("6.0.0-preview.2.21154.6");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("[green]5.0.0[/]", highlightedStableVersion);
        Assert.Equal("[red]6.0.0-preview.2.21154.6[/]", highlightedLatestVersion);
    }

    [Fact]
    public void HighlightsThePatchPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("1.0.2");
        package.LatestVersion = new("1.0.2");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("1.0.[green]2[/]", highlightedStableVersion);
        Assert.Equal("1.0.[green]2[/]", highlightedLatestVersion);
    }

    [Fact]
    public void HighlightsThePrereleasePart()
    {
        var package = new Package("test.package", new("1.0.1-rc1-final"));
        package.LatestStableVersion = new("1.0.1-rc2-final");
        package.LatestVersion = new("1.0.1-rc2-final");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("1.0.1[green]-rc2-final[/]", highlightedStableVersion);
        Assert.Equal("1.0.1[green]-rc2-final[/]", highlightedLatestVersion);
    }

    [Fact]
    public void HighlightsTheMinorPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("1.1.0");
        package.LatestVersion = new("1.1.0");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("1.[yellow]1.0[/]", highlightedStableVersion);
        Assert.Equal("1.[yellow]1.0[/]", highlightedVersion);
    }

    [Fact]
    public void HighlightsTheMajorPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("2.0.0");
        package.LatestVersion = new("2.0.0");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("[red]2.0.0[/]", highlightedStableVersion);
        Assert.Equal("[red]2.0.0[/]", highlightedLatestVersion);
    }

    [Fact]
    public void HighlightsTheMajorPartWhenCurrentVersionIsHigherThanStableVersion()
    {
        var package = new Package("test.package", new("6.0.0-preview.2.21154.6"));
        package.LatestStableVersion = new("5.0.5");
        package.LatestVersion = new("6.0.0-preview.2.21154.6");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        Assert.Equal("[red]5.0.5[/]", highlightedStableVersion);
        Assert.Equal("6.0.0-preview.2.21154.6", highlightedLatestVersion);
    }
}

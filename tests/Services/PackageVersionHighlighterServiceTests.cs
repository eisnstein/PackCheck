using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class PackageVersionHighlighterServiceTests
{
    [Test]
    public async Task HighlightsNothingWhenVersionAreTheSame()
    {
        var package = new Package("test.package", new("4.0.4"));
        package.LatestStableVersion = new("4.0.4");
        package.LatestVersion = new("4.0.4");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("4.0.4");
        await Assert.That(highlightedLatestVersion).IsEqualTo("4.0.4");
    }

    [Test]
    public async Task HighlightsGreenWhenOlderVersionIsPrelease()
    {
        var package = new Package("test.package", new("5.0.0-rc.2.20475.5"));
        package.LatestStableVersion = new("5.0.0");
        package.LatestVersion = new("6.0.0-preview.2.21154.6");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("[green]5.0.0[/]");
        await Assert.That(highlightedLatestVersion).IsEqualTo("[red]6.0.0-preview.2.21154.6[/]");
    }

    [Test]
    public async Task HighlightsThePatchPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("1.0.2");
        package.LatestVersion = new("1.0.2");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("1.0.[green]2[/]");
        await Assert.That(highlightedLatestVersion).IsEqualTo("1.0.[green]2[/]");
    }

    [Test]
    public async Task HighlightsThePrereleasePart()
    {
        var package = new Package("test.package", new("1.0.1-rc1-final"));
        package.LatestStableVersion = new("1.0.1-rc2-final");
        package.LatestVersion = new("1.0.1-rc2-final");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("1.0.1[green]-rc2-final[/]");
        await Assert.That(highlightedLatestVersion).IsEqualTo("1.0.1[green]-rc2-final[/]");
    }

    [Test]
    public async Task HighlightsTheMinorPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("1.1.0");
        package.LatestVersion = new("1.1.0");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("1.[yellow]1.0[/]");
        await Assert.That(highlightedVersion).IsEqualTo("1.[yellow]1.0[/]");
    }

    [Test]
    public async Task HighlightsTheMajorPart()
    {
        var package = new Package("test.package", new("1.0.1"));
        package.LatestStableVersion = new("2.0.0");
        package.LatestVersion = new("2.0.0");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("[red]2.0.0[/]");
        await Assert.That(highlightedLatestVersion).IsEqualTo("[red]2.0.0[/]");
    }

    [Test]
    public async Task HighlightsTheMajorPartWhenCurrentVersionIsHigherThanStableVersion()
    {
        var package = new Package("test.package", new("6.0.0-preview.2.21154.6"));
        package.LatestStableVersion = new("5.0.5");
        package.LatestVersion = new("6.0.0-preview.2.21154.6");

        var highlightedStableVersion = PackageVersionHighlighterService.HighlightLatestStableVersion(package);
        var highlightedLatestVersion = PackageVersionHighlighterService.HighlightLatestVersion(package);

        await Assert.That(highlightedStableVersion).IsEqualTo("[red]5.0.5[/]");
        await Assert.That(highlightedLatestVersion).IsEqualTo("6.0.0-preview.2.21154.6");
    }
}

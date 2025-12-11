using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Services;
using Target = PackCheck.Data.Target;

namespace PackCheck.Tests.Services;

public class SettingsServiceTests
{
    [Test]
    public async Task Returns_CheckSettingsWithConfigValues_When_NoCliValuesGiven()
    {
        var settings = new CheckSettings();
        var config = new Config()
        {
            CsProjFile = "Project.csproj",
            SlnFile = "Project.sln",
            CpmFile = "Directory.Packages.props",
            Exclude = ["Pack1"],
            Filter = ["Pack2"]
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.PathToCsProjFile).IsEqualTo(config.CsProjFile);
        await Assert.That(settings.PathToSlnFile).IsEqualTo(config.SlnFile);
        await Assert.That(settings.PathToCpmFile).IsEqualTo(config.CpmFile);
        await Assert.That(settings.Exclude).IsEquivalentTo(config.Exclude);
        await Assert.That(settings.Filter).IsEquivalentTo(config.Filter);
    }

    [Test]
    public async Task Returns_CheckSettingsUnchanged_When_SameConfigValuesAreSet()
    {
        var settings = new CheckSettings()
        {
            Exclude = ["Pack1"],
            Filter = ["Pack2"]
        };
        var config = new Config()
        {
            Exclude = ["Pack3"],
            Filter = ["Pack4"]
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        string[] expectedExclude = ["Pack1"];
        string[] expectedFilter = ["Pack2"];

        await Assert.That(settings.Exclude).IsEquivalentTo(expectedExclude);
        await Assert.That(settings.Filter).IsEquivalentTo(expectedFilter);
    }

    [Test]
    public async Task Returns_CheckSettingsUnchanged_When_SameConfigValuesAreNotSet()
    {
        var settings = new CheckSettings()
        {
            Exclude = ["Pack1"],
            Filter = ["Pack2"]
        };
        var config = new Config();

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        string[] expectedExclude = ["Pack1"];
        string[] expectedFilter = ["Pack2"];

        await Assert.That(settings.Exclude).IsEquivalentTo(expectedExclude);
        await Assert.That(settings.Filter).IsEquivalentTo(expectedFilter);
    }

    [Test]
    public async Task Returns_CheckSettingsChanged_When_ConfigValuesAreSet()
    {
        var settings = new CheckSettings();
        var config = new Config()
        {
            Exclude = ["Pack1"],
            Filter = ["Pack2"],
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        string[] expectedExclude = ["Pack1"];
        string[] expectedFilter = ["Pack2"];

        await Assert.That(settings.Exclude).IsEquivalentTo(expectedExclude);
        await Assert.That(settings.Filter).IsEquivalentTo(expectedFilter);
    }

    [Test]
    public async Task Sets_Pre_When_ConfigValueIsTrue()
    {
        var settings = new CheckSettings();
        var config = new Config()
        {
            Pre = true
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.Pre).IsTrue();
    }

    [Test]
    public async Task Sets_Pre_When_ConfigValueIsFalse_But_GivenViaCli()
    {
        var settings = new CheckSettings()
        {
            Pre = true
        };
        var config = new Config()
        {
            Pre = false
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.Pre).IsTrue();
    }

    [Test]
    public async Task DoesNotSet_Pre_When_ConfigValueIsTrue_And_CliValueIsFalse()
    {
        var settings = new CheckSettings()
        {
            Pre = false
        };
        var config = new Config()
        {
            Pre = true
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.Pre).IsFalse();
    }

    [Test]
    public async Task Returns_UpgradeSettingsWithConfigValues_When_NoCliValuesGiven()
    {
        var settings = new UpgradeSettings();
        var config = new Config()
        {
            CsProjFile = "Project.csproj",
            SlnFile = "Project.sln",
            CpmFile = "Directory.Packages.props",
            Exclude = ["Pack1"],
            Filter = ["Pack2"]
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.PathToCsProjFile).IsEqualTo(config.CsProjFile);
        await Assert.That(settings.PathToSlnFile).IsEqualTo(config.SlnFile);
        await Assert.That(settings.PathToCpmFile).IsEqualTo(config.CpmFile);
        await Assert.That(settings.Exclude).IsEquivalentTo(config.Exclude);
        await Assert.That(settings.Filter).IsEquivalentTo(config.Filter);
        await Assert.That(settings.Interactive).IsFalse();
        await Assert.That(settings.DryRun).IsFalse();
        await Assert.That(settings.Target).IsEqualTo("stable");
    }

    [Test]
    public async Task Returns_UpgradeSettingsWithCliValues_When_CliValuesGiven()
    {
        var settings = new UpgradeSettings()
        {
            PathToCsProjFile = "Project.csproj",
            PathToSlnFile = "Project.sln",
            PathToCpmFile = "Directory.Packages.props",
            Exclude = ["Pack1"],
            Filter = ["Pack2"],
            Interactive = false,
            DryRun = false,
            Target = Target.Stable
        };
        var config = new Config()
        {
            CsProjFile = "Project2.csproj",
            SlnFile = "Project2.sln",
            CpmFile = "Directory2.Packages.props",
            Exclude = ["Pack3"],
            Filter = ["Pack4"],
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        await Assert.That(settings.PathToCsProjFile).IsEqualTo("Project.csproj");
        await Assert.That(settings.PathToSlnFile).IsEqualTo("Project.sln");
        await Assert.That(settings.PathToCpmFile).IsEqualTo("Directory.Packages.props");
        await Assert.That(settings.Interactive).IsFalse();
        await Assert.That(settings.DryRun).IsFalse();
        await Assert.That(settings.Target).IsEqualTo(Target.Stable);
    }
}

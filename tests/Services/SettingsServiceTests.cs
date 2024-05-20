using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SettingsServiceTests
{
    [Fact]
    public void Returns_CheckSettingsWithConfigValues_When_NoCliValuesGiven()
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

        Assert.Equal(config.CsProjFile, settings.PathToCsProjFile);
        Assert.Equal(config.SlnFile, settings.PathToSlnFile);
        Assert.Equal(config.CpmFile, settings.PathToCpmFile);
        Assert.Equal(config.Exclude, settings.Exclude);
        Assert.Equal(config.Filter, settings.Filter);
    }

    [Fact]
    public void Returns_CheckSettingsUnchanged_When_SameConfigValuesAreSet()
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

        Assert.Equal(expectedExclude, settings.Exclude);
        Assert.Equal(expectedFilter, settings.Filter);
    }

    [Fact]
    public void Returns_CheckSettingsUnchanged_When_SameConfigValuesAreNotSet()
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

        Assert.Equal(expectedExclude, settings.Exclude);
        Assert.Equal(expectedFilter, settings.Filter);
    }

    [Fact]
    public void Returns_CheckSettingsChanged_When_ConfigValuesAreSet()
    {
        var settings = new CheckSettings();
        var config = new Config()
        {
            Exclude = ["Pack1"],
            Filter = ["Pack2"]
        };

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        string[] expectedExclude = ["Pack1"];
        string[] expectedFilter = ["Pack2"];

        Assert.Equal(expectedExclude, settings.Exclude);
        Assert.Equal(expectedFilter, settings.Filter);
    }

    [Fact]
    public void Returns_UpgradeSettingsWithConfigValues_When_NoCliValuesGiven()
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

        Assert.Equal(config.CsProjFile, settings.PathToCsProjFile);
        Assert.Equal(config.SlnFile, settings.PathToSlnFile);
        Assert.Equal(config.CpmFile, settings.PathToCpmFile);
        Assert.Equal(config.Exclude, settings.Exclude);
        Assert.Equal(config.Filter, settings.Filter);
        Assert.False(settings.Interactive);
        Assert.False(settings.DryRun);
        Assert.Equal("stable", settings.Target);
    }

    [Fact]
    public void Returns_UpgradeSettingsWithCliValues_When_CliValuesGiven()
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

        Assert.Equal("Project.csproj", settings.PathToCsProjFile);
        Assert.Equal("Project.sln", settings.PathToSlnFile);
        Assert.Equal("Directory.Packages.props", settings.PathToCpmFile);
        Assert.False(settings.Interactive);
        Assert.False(settings.DryRun);
        Assert.Equal(Target.Stable, settings.Target);
    }
}

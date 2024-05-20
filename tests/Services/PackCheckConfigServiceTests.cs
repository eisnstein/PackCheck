using System;
using System.Text.Json;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class PackCheckConfigServiceTests
{
    [Fact]
    public void ConfigFileExists()
    {
        TestHelper.LoadConfig();

        Assert.True(PackCheckConfigService.HasPackCheckConfigFile());

        TestHelper.DeleteConfig();
    }

    [Fact]
    public void Returns_FullConfig()
    {
        TestHelper.LoadConfig();

        var (_, config) = PackCheckConfigService.GetConfig();

        Assert.NotNull(config);
        Assert.Equal("Project.csproj", config.CsProjFile);
        Assert.Equal("Project.sln", config.SlnFile);
        Assert.Equal("Directory.Packages.props", config.CpmFile);
        Assert.Equal(["PackageName1"], config.Exclude);
        Assert.Equal(["PackageName2"], config.Filter);

        TestHelper.DeleteConfig();
    }

    [Fact]
    public void Returns_PartialConfig()
    {
        TestHelper.LoadConfig("partial");

        var (_, config) = PackCheckConfigService.GetConfig();

        Assert.NotNull(config);
        Assert.Null(config.SlnFile);
        Assert.Null(config.CpmFile);
        Assert.Null(config.Filter);

        Assert.Equal("Project.csproj", config.CsProjFile);
        Assert.Equal(["PackageName1"], config.Exclude);

        TestHelper.DeleteConfig();
    }

    [Fact]
    public void Throws_When_JsonIsInvalid()
    {
        TestHelper.LoadConfig("invalid");

        Action actual = () => PackCheckConfigService.GetConfig();

        Assert.Throws<JsonException>(actual);

        TestHelper.DeleteConfig();
    }
}

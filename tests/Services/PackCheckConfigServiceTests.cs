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
    public void LoadsConfigFile()
    {
        TestHelper.LoadConfig();

        var config = PackCheckConfigService.GetConfig();

        Assert.NotNull(config);
        Assert.NotNull(config.Exclude);
        Assert.NotNull(config.Filter);
        Assert.Equal(["PackageName1"], config.Exclude);
        Assert.Equal(["PackageName2"], config.Filter);

        TestHelper.DeleteConfig();
    }
}

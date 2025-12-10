using System.Text.Json;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class PackCheckConfigServiceTests
{
    [Test]
    public async Task ConfigFileExists()
    {
        TestHelper.LoadConfig();

        await Assert.That(PackCheckConfigService.HasPackCheckConfigFile()).IsTrue();

        TestHelper.DeleteConfig();
    }

    [Test]
    public async Task Returns_FullConfig()
    {
        TestHelper.LoadConfig();

        var (_, config) = PackCheckConfigService.GetConfig();

        await Assert.That(config).IsNotNull();
        await Assert.That(config.CsProjFile).IsEqualTo("Project.csproj");
        await Assert.That(config.SlnFile).IsEqualTo("Project.sln");
        await Assert.That(config.CpmFile).IsEqualTo("Directory.Packages.props");
        await Assert.That(config.Exclude).IsEquivalentTo(["PackageName1"]);
        await Assert.That(config.Filter).IsEquivalentTo(["PackageName2"]);

        TestHelper.DeleteConfig();
    }

    [Test]
    public async Task Returns_PartialConfig()
    {
        TestHelper.LoadConfig("partial");

        var (_, config) = PackCheckConfigService.GetConfig();

        await Assert.That(config).IsNotNull();
        await Assert.That(config.SlnFile).IsNull();
        await Assert.That(config.CpmFile).IsNull();
        await Assert.That(config.Filter).IsNull();
        await Assert.That(config.CsProjFile).IsEquivalentTo("Project.csproj");
        await Assert.That(config.Exclude).IsEquivalentTo(["PackageName1"]);

        TestHelper.DeleteConfig();
    }

    [Test]
    public async Task Throws_When_JsonIsInvalid()
    {
        TestHelper.LoadConfig("invalid");

        Action actual = () => PackCheckConfigService.GetConfig();

        await Assert.That(actual).ThrowsExactly<JsonException>();

        TestHelper.DeleteConfig();
    }
}

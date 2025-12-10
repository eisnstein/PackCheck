using PackCheck.Commands;
using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands.Settings;

public class UpgradeSettingsTests
{
    [Test]
    public async Task DefaultTargetVersionIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Target).IsNotNull();
        await Assert.That(settings.Target).IsEqualTo("stable");
    }

    [Test]
    public async Task TargetVersionGetsSetToStable()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", "--target", "stable"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Target).IsNotNull();
        await Assert.That(settings.Target).IsEqualTo("stable");
    }

    [Test]
    public async Task TargetVersionGetsSetToLatest()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", "--target", "latest"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Target).IsNotNull();
        await Assert.That(settings.Target).IsEqualTo("latest");
    }

    [Test]
    public async Task ThrowsOnWrongTargetVersion()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        await Assert.That(() => app.Run(["upgrade", "--target", "wrong"])).ThrowsExactly<CommandRuntimeException>();
    }

    [Test]
    public async Task AllArgumentsWithoutDryRunGetSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(
        [
            "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--target", "latest"
        ]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.PackageToUpgrade).IsNotNull();
        await Assert.That(settings.PackageToUpgrade).IsEqualTo("awesome-package");
        await Assert.That(settings.PathToCsProjFile).IsNotNull();
        await Assert.That(settings.PathToCsProjFile).IsEqualTo("\\path-to-file");
        await Assert.That(settings.Target).IsNotNull();
        await Assert.That(settings.Target).IsEqualTo("latest");
        await Assert.That(settings.DryRun).IsFalse();
        await Assert.That(settings.Interactive).IsFalse();
    }

    [Test]
    public async Task AllArgumentsWithDryRunGetSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(
        [
            "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--target", "latest", "--dry-run"
        ]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.PackageToUpgrade).IsNotNull();
        await Assert.That(settings.PackageToUpgrade).IsEqualTo("awesome-package");
        await Assert.That(settings.PathToCsProjFile).IsNotNull();
        await Assert.That(settings.PathToCsProjFile).IsEqualTo("\\path-to-file");
        await Assert.That(settings.Target).IsNotNull();
        await Assert.That(settings.Target).IsEqualTo("latest");
        await Assert.That(settings.DryRun).IsTrue();
        await Assert.That(settings.Interactive).IsFalse();
    }

    [Test]
    public async Task InteractiveIsNotSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Interactive).IsFalse();
    }

    [Test]
    public async Task InteractiveIsSetByShortCode()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", "-i"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Interactive).IsTrue();
    }

    [Test]
    public async Task InteractiveIsSetByLongCode()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", "--interactive"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<UpgradeSettings>();
        UpgradeSettings settings = (result.Settings as UpgradeSettings)!;
        await Assert.That(settings.Interactive).IsTrue();
    }
}

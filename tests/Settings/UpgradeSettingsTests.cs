using PackCheck.Commands;
using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace PackCheck.Tests.Settings;

public class UpgradeSettingsTests
{
    [Fact]
    public void DefaultTargetVersionIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[] { "upgrade" });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.NotNull(settings!.Target);
        Assert.Equal("stable", settings!.Target);
    }

    [Fact]
    public void TargetVersionGetsSetToStable()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[] { "upgrade", "--target", "stable" });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.NotNull(settings!.Target);
        Assert.Equal("stable", settings!.Target);
    }

    [Fact]
    public void TargetVersionGetsSetToLatest()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[] { "upgrade", "--target", "latest" });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.NotNull(settings!.Target);
        Assert.Equal("latest", settings!.Target);
    }

    [Fact]
    public void ThrowsOnWrongTargetVersion()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        Assert.Throws<CommandRuntimeException>(() => app.Run(new[] { "upgrade", "--target", "wrong" }));
    }

    [Fact]
    public void AllArgumentsWithoutDryRunGetSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[]
        {
            "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--target", "latest"
        });

        Assert.Equal(-1, result.ExitCode);
        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.NotNull(settings!.PackageToUpgrade);
        Assert.Equal("awesome-package", settings!.PackageToUpgrade);
        Assert.NotNull(settings!.PathToCsProjFile);
        Assert.Equal("\\path-to-file", settings!.PathToCsProjFile);
        Assert.NotNull(settings!.Target);
        Assert.Equal("latest", settings!.Target);
        Assert.False(settings!.DryRun);
        Assert.False(settings!.Interactive);
    }

    [Fact]
    public void AllArgumentsWithDryRunGetSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[]
        {
            "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--target", "latest", "--dry-run"
        });

        Assert.Equal(-1, result.ExitCode);
        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.NotNull(settings!.PackageToUpgrade);
        Assert.Equal("awesome-package", settings!.PackageToUpgrade);
        Assert.NotNull(settings!.PathToCsProjFile);
        Assert.Equal("\\path-to-file", settings!.PathToCsProjFile);
        Assert.NotNull(settings!.Target);
        Assert.Equal("latest", settings!.Target);
        Assert.True(settings!.DryRun);
        Assert.False(settings!.Interactive);
    }

    [Fact]
    public void InteractiveIsNotSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[]
        {
            "upgrade"
        });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.False(settings!.Interactive);
    }

    [Fact]
    public void InteractiveIsSetByShortCode()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[]
        {
            "upgrade", "-i"
        });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.True(settings!.Interactive);
    }

    [Fact]
    public void InteractiveIsSetByLongCode()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(new[]
        {
            "upgrade", "--interactive"
        });

        Assert.NotNull(result.Settings);
        Assert.IsType<UpgradeSettings>(result.Settings);
        UpgradeSettings? settings = result.Settings as UpgradeSettings;
        Assert.True(settings!.Interactive);
    }
}

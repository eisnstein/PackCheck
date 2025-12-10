using PackCheck.Commands;
using PackCheck.Commands.Settings;
using PackCheck.Services;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands.Settings;

public class CheckSettingsTests
{
    [Test]
    public async Task PathToCsProjFileIsNotSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.PathToCsProjFile).IsNull();
    }

    [Test]
    public async Task PathToCsProjFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--csprojFile"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'csprojFile' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task PathToCsProjFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--csprojFile", @".\example.csproj"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.PathToCsProjFile).IsNotNull();
        await Assert.That(settings.PathToCsProjFile).IsEqualTo(@".\example.csproj");
    }

    [Test]
    public async Task PathToSolutionFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--slnFile"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'slnFile' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task PathToSlnFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--slnFile", @".\example.sln"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.PathToSlnFile).IsNotNull();
        await Assert.That(settings.PathToSlnFile).IsEqualTo(@".\example.sln");
    }

    [Test]
    public async Task PathToSolutionXFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--slnxFile"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'slnxFile' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task PathToSolutionXFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--slnxFile", @".\example.slnx"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.PathToSlnxFile).IsNotNull();
        await Assert.That(settings.PathToSlnxFile).IsEqualTo(@".\example.slnx");
    }

    [Test]
    public async Task PathToCpmFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--cpmFile"]);

        // No Directory.Packages.props file is given
        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'cpmFile' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task PathToCpmFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--cpmFile", $".\\{CentralPackageMgmtService.CpmFileName}"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.PathToCpmFile).IsNotNull();
        await Assert.That(settings.PathToCpmFile).IsEqualTo($".\\{CentralPackageMgmtService.CpmFileName}");
    }

    [Test]
    public async Task FilterIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-f", "Package2"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.Filter).IsNotNull();

        string[] expected = ["Package1", "Package2"];
        await Assert.That(settings.Filter).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Error_When_FilterOptionIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-f"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'filter' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task ExcludeIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--exclude", "Package1", "-x", "Package2"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.Exclude).IsNotNull();

        string[] expected = ["Package1", "Package2"];
        await Assert.That(settings.Exclude).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Error_When_ExcludeOptionIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-x"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith(
            "Error: Option 'exclude' is defined but no value has been provided."
        );
    }

    [Test]
    public async Task FormatOptionIsGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--format", "group"]);

        await Assert.That(result.Settings).IsNotNull();
        await Assert.That(result.Settings).IsTypeOf<CheckSettings>();
        CheckSettings settings = (result.Settings as CheckSettings)!;
        await Assert.That(settings.Format).IsNotNull();
        await Assert.That(settings.Format).IsEqualTo("group");
    }

    [Test]
    public async Task Error_When_WrongFormatOptionIsGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        await Assert.That(() => app.Run(["check", "--format", "wrong"])).ThrowsExactly<CommandRuntimeException>();
    }
}

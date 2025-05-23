using PackCheck.Commands;
using PackCheck.Commands.Settings;
using PackCheck.Services;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace PackCheck.Tests.Commands.Settings;

public class CheckSettingsTests
{
    [Fact]
    public void PathToCsProjFileIsNotSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check" });

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.Null(settings.PathToCsProjFile);
    }

    [Fact]
    public void PathToCsProjFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--csprojFile" });

        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'csprojFile' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void PathToCsProjFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--csprojFile", @".\example.csproj" });

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.PathToCsProjFile);
        Assert.Equal(@".\example.csproj", settings.PathToCsProjFile);
    }

    [Fact]
    public void PathToSolutionFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--slnFile" });

        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'slnFile' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void PathToSlnFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            // config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--slnFile", @".\example.sln" });

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.PathToSlnFile);
        Assert.Equal(@".\example.sln", settings.PathToSlnFile);
    }

    [Fact]
    public void PathToSolutionXFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--slnxFile" });

        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'slnxFile' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void PathToSolutionXFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            // config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--slnxFile", @".\example.slnx" });

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.PathToSlnxFile);
        Assert.Equal(@".\example.slnx", settings.PathToSlnxFile);
    }

    [Fact]
    public void PathToCpmFileIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--cpmFile" });

        // No Directory.Packages.props file is given
        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'cpmFile' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void PathToCpmFileIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--cpmFile", $".\\{CentralPackageMgmtService.CpmFileName}"]);

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.PathToCpmFile);
        Assert.Equal($".\\{CentralPackageMgmtService.CpmFileName}", settings.PathToCpmFile);
    }

    [Fact]
    public void FilterIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-f", "Package2"]);

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.Filter);

        string[] expected = ["Package1", "Package2"];
        Assert.Equal(expected, settings.Filter);
    }

    [Fact]
    public void Error_When_FilterOptionIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-f"]);

        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'filter' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void ExcludeIsSet()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--exclude", "Package1", "-x", "Package2"]);

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.Exclude);

        string[] expected = ["Package1", "Package2"];
        Assert.Equal(expected, settings.Exclude);
    }

    [Fact]
    public void Error_When_ExcludeOptionIsSetButNoValueGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Package1", "-x"]);

        Assert.Equal(-1, result.ExitCode);
        Assert.StartsWith(
            "Error: Option 'exclude' is defined but no value has been provided.",
            result.Output
        );
    }

    [Fact]
    public void FormatOptionIsGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--format", "group"]);

        Assert.NotNull(result.Settings);
        Assert.IsType<CheckSettings>(result.Settings);
        CheckSettings settings = (result.Settings as CheckSettings)!;
        Assert.NotNull(settings.Format);

        Assert.Equal("group", settings.Format);
    }

    [Fact]
    public void Error_When_WrongFormatOptionIsGiven()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        Assert.Throws<CommandRuntimeException>(() => app.Run(new[] { "check", "--format", "wrong" }));
    }
}

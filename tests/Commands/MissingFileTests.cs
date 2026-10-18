using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

/// <summary>
/// A file given via an option that does not exist must lead to a warning
/// and exit code -1, not to an unhandled exception.
/// PropagateExceptions is needed, otherwise Spectre catches unhandled
/// exceptions itself and also returns -1.
/// </summary>
public class MissingFileTests
{
    [Test]
    [Arguments("--csprojFile", "missing.csproj")]
    [Arguments("--slnFile", "missing.sln")]
    [Arguments("--slnxFile", "missing.slnx")]
    [Arguments("--cpmFile", "missing.props")]
    [Arguments("--fbaFile", "missing.cs")]
    public async Task Check_ReturnsError_When_GivenFileDoesNotExist(string option, string path)
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", option, path]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
    }

    [Test]
    [Arguments("--csprojFile", "missing.csproj")]
    [Arguments("--slnFile", "missing.sln")]
    [Arguments("--slnxFile", "missing.slnx")]
    [Arguments("--cpmFile", "missing.props")]
    [Arguments("--fbaFile", "missing.cs")]
    public async Task Upgrade_ReturnsError_When_GivenFileDoesNotExist(string option, string path)
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", option, path]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
    }
}

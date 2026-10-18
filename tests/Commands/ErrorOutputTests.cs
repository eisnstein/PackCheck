using PackCheck.Commands;
using PackCheck.Tests.Fakes;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

/// <summary>
/// Errors must lead to a warning and exit code -1, not to an unhandled exception.
/// PropagateExceptions is needed, otherwise Spectre catches unhandled
/// exceptions itself and also returns -1.
/// </summary>
public class ErrorOutputTests
{
    [Before(Test)]
    public void Setup()
    {
        TestHelper.LoadTestCsProjFile();
    }

    [After(Test)]
    public void TearDown()
    {
        TestHelper.DeleteTestCsProjFile();
        TestHelper.DeleteConfig();
    }

    [Test]
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task ReturnsError_When_NuGetFeedIsUnreachable(string command)
    {
        var app = CreateApp(new UnreachableNuGetApiService());

        CommandAppResult result = app.Run([command]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
    }

    [Test]
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task ReturnsError_When_ConfigFileIsInvalid(string command)
    {
        // The warning contains ".packcheckrc[.json]", which must not be parsed as markup
        TestHelper.LoadConfig("invalid");
        var app = CreateApp();

        CommandAppResult result = app.Run([command]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
    }

    [Test]
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task ReturnsError_When_MissingFileNameContainsBrackets(string command)
    {
        // The file name is part of the warning, "[1]" must not be parsed as markup
        var app = CreateApp();

        CommandAppResult result = app.Run([command, "--csprojFile", "missing[1].csproj"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
    }

    private static CommandAppTester CreateApp(PackCheck.Services.INuGetApiService? nuGetApiService = null)
    {
        var app = TestHelper.CreateCommandAppTester(nuGetApiService);
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        return app;
    }
}

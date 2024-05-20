using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using VerifyXunit;

namespace PackCheck.Tests.Commands;

public class CheckCommandTest
{
    [Fact]
    public void Returns_Success_ForCheck()
    {
        TestHelper.LoadTestCsProjFile();

        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run("check");

        Verifier.Verify(result);

        TestHelper.DeleteTestCsProjFile();
    }

    [Fact]
    public void Returns_Success_ForCheckWithFilter()
    {
        TestHelper.LoadTestCsProjFile();

        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("check");
        });

        CommandAppResult result = app.Run(new[] { "check", "--filter", "Spectre.Console" });

        Verifier.Verify(result);

        TestHelper.DeleteTestCsProjFile();
    }
}

using PackCheck.Commands;
using Spectre.Console.Cli.Testing;
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
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Spectre.Console"]);

        Verifier.Verify(result);

        TestHelper.DeleteTestCsProjFile();
    }
}

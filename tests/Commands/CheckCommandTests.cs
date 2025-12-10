using PackCheck.Commands;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

public class CheckCommandTest
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
    }

    [Test]
    public Task Returns_Success_ForCheck()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run("check");

        return Verifier.Verify(result);
    }

    [Test]
    public Task Returns_Success_ForCheckWithFilter()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Spectre.Console"]);

        return Verifier.Verify(result);
    }
}

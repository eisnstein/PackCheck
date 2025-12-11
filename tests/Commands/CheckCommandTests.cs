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
    public Task Returns_Success_ForCheck_Without_Pre()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run("check");

        return Verify(result);
    }

    [Test]
    public Task Returns_Success_ForCheck_With_Pre()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--pre"]);

        return Verify(result);
    }

    [Test]
    public Task Returns_Success_ForCheckWithFilter_Without_Pre()
    {
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--filter", "Spectre.Console"]);

        return Verify(result);
    }
}

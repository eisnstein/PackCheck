using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

/// <summary>
/// Behaviour shared by check and upgrade through PackageCommand.
/// </summary>
public class PackageCommandTests
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
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task ReturnsSuccess_When_NothingIsLeftToDo(string command)
    {
        // "Nothing to do" is not an error, also with an explicitly given project
        var app = CreateApp();

        CommandAppResult result = app.Run([command, "--csprojFile", "test.csproj", "--exclude", "*"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("No packages to check.");
    }

    [Test]
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task ChecksAllProjectsOfSolution_BeforeProcessingAny(string command)
    {
        // The first project exists, the second does not
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "partial-solution");
        Directory.CreateDirectory(Path.Combine(directory, "Exists"));
        File.Copy("TestData/test.csproj", Path.Combine(directory, "Exists", "Exists.csproj"), true);
        await File.WriteAllTextAsync(Path.Combine(directory, "partial.slnx"), """
            <Solution>
              <Project Path="Exists\Exists.csproj" />
              <Project Path="Missing\Missing.csproj" />
            </Solution>
            """);

        try
        {
            var app = CreateApp();

            // Dry run for upgrade, so the fixture is not changed if the test fails
            string[] args = command == "upgrade"
                ? [command, "--slnxFile", "partial-solution/partial.slnx", "--dry-run"]
                : [command, "--slnxFile", "partial-solution/partial.slnx"];

            CommandAppResult result = app.Run(args);

            await Assert.That(result.ExitCode).IsEqualTo(-1);
            await Assert.That(result.Output).Contains("Missing.csproj");
            // Nothing was checked or upgraded before the error
            await Assert.That(result.Output).DoesNotContain("Exists.csproj");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static CommandAppTester CreateApp()
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        return app;
    }
}

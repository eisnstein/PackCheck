using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

/// <summary>
/// The project paths in a solution are relative to the solution file,
/// so a solution in a subdirectory of the current directory must work.
/// </summary>
public class SolutionInSubdirectoryTests
{
    [Test]
    [Arguments("--slnFile", "TestData/solution/testSolution.sln")]
    [Arguments("--slnxFile", "TestData/solutionx/testSolution.slnx")]
    public async Task Check_FindsProjects_When_SolutionIsInSubdirectory(string option, string path)
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", option, path]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        // SolProj.csproj references NuGet.Protocol 6.2.1, which has an upgrade in the fake
        await Assert.That(result.Output).Contains("NuGet.Protocol");
    }

    [Test]
    [Arguments("--slnFile", "TestData/solution/testSolution.sln")]
    [Arguments("--slnxFile", "TestData/solutionx/testSolution.slnx")]
    public async Task Upgrade_FindsProjects_When_SolutionIsInSubdirectory(string option, string path)
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        // Dry run, so the shared fixture files are not changed
        CommandAppResult result = app.Run(["upgrade", option, path, "--dry-run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
    }
}

using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SolutionXFileServiceTests
{
    [Test]
    public async Task LoadsSolutionXFile()
    {
        TestHelper.LoadSolutionX();

        await Assert.That(SolutionXFileService.HasSolutionX()).IsTrue();

        TestHelper.DeleteSolutionX();
    }

    [Test]
    public async Task ParseProjectDefinitions()
    {
        TestHelper.LoadSolutionX();
        List<string> expectedPaths = [
            Path.Combine("src", "SolProj.csproj"),
            Path.Combine("tests", "SolProj.Tests.csproj")
        ];

        var projectPaths = SolutionXFileService.ParseProjectDefinitions("testSolution.slnx");

        await Assert.That(projectPaths.Count).IsEqualTo(2);
        await Assert.That(projectPaths).IsEquivalentTo(expectedPaths);

        TestHelper.DeleteSolutionX();
    }
}

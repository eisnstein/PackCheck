using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SolutionFileServiceTests
{
    [Test]
    public async Task LoadsSolutionFile()
    {
        TestHelper.LoadSolution();

        await Assert.That(SolutionFileService.HasSolution()).IsTrue();

        TestHelper.DeleteSolution();
    }

    [Test]
    public async Task GetProjectDefinitionsFromSolutionFile()
    {
        TestHelper.LoadSolution();

        var projectDefinitions = SolutionFileService.GetProjectDefinitions("testSolution.sln").ToList();

        await Assert.That(projectDefinitions.Count).IsEqualTo(2);
        foreach (var projectDefinition in projectDefinitions)
        {
            await Assert.That(projectDefinition).StartsWith("Project(");
        }

        TestHelper.DeleteSolution();
    }

    [Test]
    public async Task ParseProjectDefinitions()
    {
        TestHelper.LoadSolution();
        // The service converts the backslashes from the .sln file to the platform's separator
        List<string> expectedPaths = [
            Path.Combine("SolProj", "SolProj.csproj"),
            Path.Combine("SolProj.Tests", "SolProj.Tests.csproj")
        ];

        var projectDefinitions = SolutionFileService.GetProjectDefinitions("testSolution.sln");
        var projectPaths = SolutionFileService.ParseProjectDefinitions(projectDefinitions);

        await Assert.That(projectPaths.Count).IsEqualTo(2);
        await Assert.That(projectPaths).IsEquivalentTo(expectedPaths);

        TestHelper.DeleteSolution();
    }

    [Test]
    public async Task GetCsProjFiles_ResolvesPathsRelativeToSolutionFile()
    {
        // The solution is in a subdirectory, not in the current directory
        var csProjFiles = SolutionFileService.GetCsProjFiles(Path.Combine("TestData", "solution", "testSolution.sln"));

        var solutionDirectory = Path.GetFullPath(Path.Combine("TestData", "solution"));
        await Assert.That(csProjFiles).IsEquivalentTo([
            Path.Combine(solutionDirectory, "SolProj", "SolProj.csproj"),
            Path.Combine(solutionDirectory, "SolProj.Tests", "SolProj.Tests.csproj"),
        ]);
        await Assert.That(csProjFiles.All(File.Exists)).IsTrue();
    }
}

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
        // The service converts the backslashes from the .slnx file to the platform's separator
        List<string> expectedPaths = [
            Path.Combine("src", "SolProj.csproj"),
            Path.Combine("tests", "SolProj.Tests.csproj")
        ];

        var projectPaths = SolutionXFileService.ParseProjectDefinitions("testSolution.slnx");

        await Assert.That(projectPaths.Count).IsEqualTo(2);
        await Assert.That(projectPaths).IsEquivalentTo(expectedPaths);

        TestHelper.DeleteSolutionX();
    }

    [Test]
    public async Task GetCsProjFiles_ResolvesPathsRelativeToSolutionFile()
    {
        // The solution is in a subdirectory, not in the current directory
        var csProjFiles = SolutionXFileService.GetCsProjFiles(Path.Combine("TestData", "solutionx", "testSolution.slnx"));

        var solutionDirectory = Path.GetFullPath(Path.Combine("TestData", "solutionx"));
        await Assert.That(csProjFiles).IsEquivalentTo([
            Path.Combine(solutionDirectory, "src", "SolProj.csproj"),
            Path.Combine(solutionDirectory, "tests", "SolProj.Tests.csproj"),
        ]);
        await Assert.That(csProjFiles.All(File.Exists)).IsTrue();
    }
}

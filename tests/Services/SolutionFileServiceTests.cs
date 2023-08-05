using System.Collections.Generic;
using System.Linq;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SolutionFileServiceTests
{
    [Fact]
    public void LoadsSolutionFile()
    {
        TestHelper.LoadSolution();

        Assert.True(SolutionFileService.HasSolution());

        TestHelper.DeleteSolution();
    }

    [Fact]
    public void GetProjectDefinitionsFromSolutionFile()
    {
        TestHelper.LoadSolution();

        var projectDefinitions = SolutionFileService.GetProjectDefinitions("testSolution.sln").ToList();

        Assert.Equal(2, projectDefinitions.Count);
        foreach (var projectDefinition in projectDefinitions)
        {
            Assert.StartsWith("Project(", projectDefinition);
        }

        TestHelper.DeleteSolution();
    }

    [Fact]
    public void ParseProjectDefinitions()
    {
        TestHelper.LoadSolution();
        List<string> expectedPaths = new()
        {
            @"SolProj\SolProj.csproj",
            @"SolProj.Tests\SolProj.Tests.csproj"
        };

        var projectDefinitions = SolutionFileService.GetProjectDefinitions("testSolution.sln");
        var projectPaths = SolutionFileService.ParseProjectDefinitions(projectDefinitions);

        Assert.Equal(2, projectPaths.Count);
        Assert.Equal(expectedPaths, projectPaths);

        TestHelper.DeleteSolution();
    }
}

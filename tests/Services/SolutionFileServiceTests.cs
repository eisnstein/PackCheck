using System.Collections.Generic;
using System.Linq;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SolutionFileServiceTests
{
    private readonly SolutionFileService _service;

    public SolutionFileServiceTests()
    {
        _service = new();
    }

    [Fact]
    public void LoadsSolutionFile()
    {
        TestHelper.LoadSolution();

        Assert.True(_service.HasSolution());
    }

    [Fact]
    public void GetProjectDefinitionsFromSolutionFile()
    {
        TestHelper.LoadSolution();

        var projectDefinitions = _service.GetProjectDefinitions("testSolution.sln").ToList();

        Assert.Equal(2, projectDefinitions.Count);
        foreach (var projectDefinition in projectDefinitions)
        {
            Assert.StartsWith("Project(", projectDefinition);
        }
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

        var projectDefinitions = _service.GetProjectDefinitions("testSolution.sln");
        var projectPaths = _service.ParseProjectDefinitions(projectDefinitions);

        Assert.Equal(2, projectPaths.Count);
        Assert.Equal(expectedPaths, projectPaths);
    }
}

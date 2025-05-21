using System.Collections.Generic;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class SolutionXFileServiceTests
{
    [Fact]
    public void LoadsSolutionXFile()
    {
        TestHelper.LoadSolutionX();

        Assert.True(SolutionXFileService.HasSolutionX());

        TestHelper.DeleteSolutionX();
    }

    [Fact]
    public void ParseProjectDefinitions()
    {
        TestHelper.LoadSolutionX();
        List<string> expectedPaths = [
            @"src\SolProj.csproj",
            @"tests\SolProj.Tests.csproj"
        ];

        var projectPaths = SolutionXFileService.ParseProjectDefinitions("testSolution.slnx");

        Assert.Equal(2, projectPaths.Count);
        Assert.Equal(expectedPaths, projectPaths);

        TestHelper.DeleteSolutionX();
    }
}

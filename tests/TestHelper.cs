using System.IO;
using PackCheck.Services;
using PackCheck.Tests.Fakes;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace PackCheck.Tests;

public static class TestHelper
{
    public static CommandAppTester CreateCommandAppTester(INuGetApiService? nuGetApiService = null)
    {
        var registrar = new TestTypeRegistrar();
        registrar.RegisterInstance(typeof(INuGetApiService), nuGetApiService ?? new FakeNuGetApiService());

        // Wide enough that paths in the output are not wrapped,
        // so Verify can replace them with {CurrentDirectory}
        var console = new TestConsole().Width(1000);

        return new CommandAppTester(registrar, console: console);
    }

    public static void LoadTestCsProjFile()
    {
        File.Copy("TestData/test.csproj", "test.csproj", true);
    }

    public static void DeleteTestCsProjFile()
    {
        if (File.Exists("test.csproj"))
        {
            File.Delete("test.csproj");
        }
    }

    public static void LoadSolution()
    {
        File.Copy("TestData/solution/testSolution.sln", "testSolution.sln", true);
    }

    public static void DeleteSolution()
    {
        if (File.Exists("testSolution.sln"))
        {
            File.Delete("testSolution.sln");
        }
    }

    public static void LoadSolutionX()
    {
        File.Copy("TestData/solutionx/testSolution.slnx", "testSolution.slnx", true);
    }

    public static void DeleteSolutionX()
    {
        if (File.Exists("testSolution.slnx"))
        {
            File.Delete("testSolution.slnx");
        }
    }

    public static void LoadCpm()
    {
        File.Copy("TestData/cpm/Directory.Packages.props", "Directory.Packages.props", true);
    }

    public static void DeleteCpm()
    {
        if (File.Exists("Directory.Packages.props"))
        {
            File.Delete("Directory.Packages.props");
        }
    }

    public static void LoadFba()
    {
        File.Copy("TestData/fba/app.cs", "app.cs", true);
    }

    public static void DeleteFba()
    {
        if (File.Exists("app.cs"))
        {
            File.Delete("app.cs");
        }
    }

    public static void LoadConfig(string type = "full")
    {
        File.Copy($"TestData/{type}.config.json", ".packcheckrc.json", true);
    }

    public static void DeleteConfig()
    {
        if (File.Exists(".packcheckrc.json"))
        {
            File.Delete(".packcheckrc.json");
        }
    }
}

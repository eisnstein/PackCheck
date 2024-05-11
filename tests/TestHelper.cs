using System.IO;

namespace PackCheck.Tests;

public static class TestHelper
{
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

    public static void LoadConfig()
    {
        File.Copy("TestData/.packcheckrc.json", ".packcheckrc.json", true);
    }

    public static void DeleteConfig()
    {
        if (File.Exists(".packcheckrc.json"))
        {
            File.Delete(".packcheckrc.json");
        }
    }
}

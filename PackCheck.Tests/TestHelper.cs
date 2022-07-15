using System.IO;

namespace PackCheck.Tests;

public static class TestHelper
{
    public static void ResetTestCsProjFile()
    {
        File.Copy("TestData/test.csproj", "test.csproj", true);
    }
}

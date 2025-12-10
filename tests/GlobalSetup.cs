// Here you could define global logic that would affect all tests

// You can use attributes at the assembly level to apply to all tests in the assembly
using DiffEngine;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[assembly: NotInParallel]

namespace PackCheck.Tests;

public class GlobalHooks
{
    [Before(TestSession)]
    public static void SetUp()
    {
        DiffTools.UseOrder(DiffTool.VisualStudioCode);
        Verifier.UseProjectRelativeDirectory("Snapshots");
    }

    [After(TestSession)]
    public static void CleanUp()
    {
    }
}

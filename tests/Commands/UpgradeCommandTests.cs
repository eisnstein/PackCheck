using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

public class UpgradeCommandTests
{
    [Before(Test)]
    public void Setup()
    {
        TestHelper.LoadTestCsProjFile();
    }

    [After(Test)]
    public void TearDown()
    {
        TestHelper.DeleteTestCsProjFile();
        TestHelper.DeleteConfig();
    }

    [Test]
    public async Task Upgrade_UsesCpmFileFromConfig_InsteadOfCsProjInCurrentDirectory()
    {
        // test.csproj is in the current directory, but the config points to a
        // CPM file with a non-default name, which is not auto-detected.
        var pathToCpmFile = Path.Combine(Directory.GetCurrentDirectory(), "custom.props");
        await File.WriteAllTextAsync(pathToCpmFile, """
            <Project>
              <ItemGroup>
                <PackageVersion Include="NuGet.Protocol" Version="6.2.1" />
              </ItemGroup>
            </Project>
            """);
        await File.WriteAllTextAsync(".packcheckrc.json", """{ "CpmFile": "custom.props" }""");
        var csProjBefore = await File.ReadAllTextAsync("test.csproj");

        try
        {
            var app = TestHelper.CreateCommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(["upgrade"]);

            await Assert.That(result.ExitCode).IsEqualTo(0);
            await Assert.That(await File.ReadAllTextAsync(pathToCpmFile)).Contains("<PackageVersion Include=\"NuGet.Protocol\" Version=\"7.6.0\" />");
            await Assert.That(await File.ReadAllTextAsync("test.csproj")).IsEqualTo(csProjBefore);
        }
        finally
        {
            File.Delete(pathToCpmFile);
        }
    }

    [Test]
    public async Task Upgrade_MatchesPackageNameCaseInsensitive()
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        CommandAppResult result = app.Run(["upgrade", "nuget.protocol"]);

        var fileContent = await File.ReadAllTextAsync("test.csproj");
        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(fileContent).Contains("<PackageReference Include=\"NuGet.Protocol\" Version=\"7.6.0\" />");
        await Assert.That(fileContent).Contains("<PackageReference Include=\"NuGet.Versioning\" Version=\"6.2.1\" />");
    }
}

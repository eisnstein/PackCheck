using System.Text.Json;
using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace PackCheck.Tests.Commands;

/// <summary>
/// --output json/markdown: stdout only contains the one document with the result.
/// </summary>
public class StructuredOutputTests
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
    public Task Check_Json()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "json"]);

        return Verify(result.Output);
    }

    [Test]
    public Task Check_Markdown()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "markdown"]);

        return Verify(result.Output);
    }

    [Test]
    public Task Check_Markdown_With_Pre()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "markdown", "--pre"]);

        return Verify(result.Output);
    }

    [Test]
    public async Task Check_Markdown_Grouped()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "markdown", "--format", "group"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output.ReplaceLineEndings("\n")).IsEqualTo("""
            ## `test.csproj`

            ### Patch

            _Backwards compatible - bug fixes_

            | Package | Current | Latest stable | Upgrade |
            |---|--:|--:|---|
            | Spectre.Console | 0.44.0 | 0.44.1 | patch |

            ### Minor

            _Backwards compatible - new features_

            | Package | Current | Latest stable | Upgrade |
            |---|--:|--:|---|
            | NuGet.Versioning | 6.2.1 | 6.3.0 | minor |

            ### Major

            _Possibly breaking changes - check Changelog_

            | Package | Current | Latest stable | Upgrade |
            |---|--:|--:|---|
            | NuGet.Protocol | 6.2.1 | 7.6.0 | major |
            """.ReplaceLineEndings("\n"));
    }

    [Test]
    public async Task Check_Json_Grouped_IsNotChanged()
    {
        CommandAppResult grouped = CreateApp().Run(["check", "--output", "json", "--format", "group"]);
        CommandAppResult flat = CreateApp().Run(["check", "--output", "json"]);

        await Assert.That(grouped.ExitCode).IsEqualTo(0);
        await Assert.That(grouped.Output).IsEqualTo(flat.Output);
    }

    [Test]
    public async Task Check_Json_IsTheOnlyOutput()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "json"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);

        // Parsing fails if anything else was printed
        using var json = JsonDocument.Parse(result.Output);
        var root = json.RootElement;
        await Assert.That(root.GetProperty("command").GetString()).IsEqualTo("check");
        await Assert.That(root.GetProperty("error").ValueKind).IsEqualTo(JsonValueKind.Null);

        var file = root.GetProperty("files")[0];
        await Assert.That(file.GetProperty("path").GetString()).IsEqualTo("test.csproj");

        var protocol = file.GetProperty("packages").EnumerateArray()
            .Single(p => p.GetProperty("name").GetString() == "NuGet.Protocol");
        await Assert.That(protocol.GetProperty("currentVersion").GetString()).IsEqualTo("6.2.1");
        await Assert.That(protocol.GetProperty("latestStableVersion").GetString()).IsEqualTo("7.6.0");
        await Assert.That(protocol.GetProperty("upgradeType").GetString()).IsEqualTo("major");
    }

    [Test]
    public async Task Check_Json_FromConfig()
    {
        await File.WriteAllTextAsync(".packcheckrc.json", """{ "Output": "json" }""");

        CommandAppResult result = CreateApp().Run(["check"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        using var json = JsonDocument.Parse(result.Output);
        await Assert.That(json.RootElement.GetProperty("files").GetArrayLength()).IsEqualTo(1);
    }

    [Test]
    public async Task Error_When_OutputInConfigIsInvalid()
    {
        await File.WriteAllTextAsync(".packcheckrc.json", """{ "Output": "xml" }""");

        CommandAppResult result = CreateApp().Run(["check"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).Contains("Output value 'xml' in the config file is not valid.");
    }

    [Test]
    public async Task Check_Json_WithAllPackagesFilteredOut_HasEmptyPackages()
    {
        CommandAppResult result = CreateApp().Run(["check", "--output", "json", "--exclude", "*"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        using var json = JsonDocument.Parse(result.Output);
        var file = json.RootElement.GetProperty("files")[0];
        await Assert.That(file.GetProperty("packages").GetArrayLength()).IsEqualTo(0);
    }

    [Test]
    [Arguments("check")]
    [Arguments("upgrade")]
    public async Task Json_ContainsTheError_When_FileIsMissing(string command)
    {
        CommandAppResult result = CreateApp().Run([command, "--output", "json", "--csprojFile", "missing[1].csproj"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        using var json = JsonDocument.Parse(result.Output);
        var error = json.RootElement.GetProperty("error").GetString();
        // Plain text, without markup
        await Assert.That(error).Contains("missing[1].csproj");
        await Assert.That(error).DoesNotContain("[white]");
    }

    [Test]
    public async Task Markdown_ContainsTheError_When_FeedIsUnreachable()
    {
        var app = TestHelper.CreateCommandAppTester(new Fakes.UnreachableNuGetApiService());
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        CommandAppResult result = app.Run(["check", "--output", "markdown"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(result.Output).StartsWith("**Error:** Could not fetch package versions from NuGet.");
    }

    [Test]
    public async Task Check_Json_ForSolution_IsOneDocument()
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "json-solution");
        Directory.CreateDirectory(Path.Combine(directory, "One"));
        Directory.CreateDirectory(Path.Combine(directory, "Two"));
        File.Copy("TestData/test.csproj", Path.Combine(directory, "One", "One.csproj"), true);
        File.Copy("TestData/test.csproj", Path.Combine(directory, "Two", "Two.csproj"), true);
        await File.WriteAllTextAsync(Path.Combine(directory, "json.slnx"), """
            <Solution>
              <Project Path="One\One.csproj" />
              <Project Path="Two\Two.csproj" />
            </Solution>
            """);

        try
        {
            CommandAppResult result = CreateApp().Run(["check", "--output", "json", "--slnxFile", "json-solution/json.slnx"]);

            await Assert.That(result.ExitCode).IsEqualTo(0);
            using var json = JsonDocument.Parse(result.Output);
            var paths = json.RootElement.GetProperty("files").EnumerateArray()
                .Select(f => f.GetProperty("path").GetString()!)
                .ToList();
            await Assert.That(paths).IsEquivalentTo([
                Path.Combine("json-solution", "One", "One.csproj"),
                Path.Combine("json-solution", "Two", "Two.csproj")
            ]);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task Upgrade_Json_DryRun_DoesNotChangeTheFile()
    {
        var before = await File.ReadAllTextAsync("test.csproj");

        CommandAppResult result = CreateApp().Run(["upgrade", "--output", "json", "--dry-run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(await File.ReadAllTextAsync("test.csproj")).IsEqualTo(before);

        using var json = JsonDocument.Parse(result.Output);
        var root = json.RootElement;
        await Assert.That(root.GetProperty("command").GetString()).IsEqualTo("upgrade");
        await Assert.That(root.GetProperty("target").GetString()).IsEqualTo("stable");
        await Assert.That(root.GetProperty("dryRun").GetBoolean()).IsTrue();

        var protocol = root.GetProperty("files")[0].GetProperty("packages").EnumerateArray()
            .Single(p => p.GetProperty("name").GetString() == "NuGet.Protocol");
        await Assert.That(protocol.GetProperty("currentVersion").GetString()).IsEqualTo("6.2.1");
        await Assert.That(protocol.GetProperty("newVersion").GetString()).IsEqualTo("7.6.0");
        await Assert.That(protocol.GetProperty("upgradeType").GetString()).IsEqualTo("major");
    }

    [Test]
    public async Task Upgrade_Json_ChangesTheFile()
    {
        CommandAppResult result = CreateApp().Run(["upgrade", "--output", "json"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(await File.ReadAllTextAsync("test.csproj"))
            .Contains("<PackageReference Include=\"NuGet.Protocol\" Version=\"7.6.0\" />");

        using var json = JsonDocument.Parse(result.Output);
        await Assert.That(json.RootElement.GetProperty("dryRun").GetBoolean()).IsFalse();
    }

    [Test]
    public async Task Upgrade_Markdown_DryRun()
    {
        CommandAppResult result = CreateApp().Run(["upgrade", "NuGet.Protocol", "--output", "markdown", "--dry-run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output.ReplaceLineEndings("\n")).IsEqualTo("""
            _Dry run: no files were changed._

            ## `test.csproj`

            | Package | Current | New | Upgrade |
            |---|--:|--:|---|
            | NuGet.Protocol | 6.2.1 | 7.6.0 | major |
            """.ReplaceLineEndings("\n"));
    }

    [Test]
    public async Task Error_When_InteractiveWithStructuredOutput()
    {
        var before = await File.ReadAllTextAsync("test.csproj");

        CommandAppResult result = CreateApp().Run(["upgrade", "-i", "--output", "json"]);

        await Assert.That(result.ExitCode).IsEqualTo(-1);
        await Assert.That(await File.ReadAllTextAsync("test.csproj")).IsEqualTo(before);
        using var json = JsonDocument.Parse(result.Output);
        await Assert.That(json.RootElement.GetProperty("error").GetString())
            .IsEqualTo("--interactive cannot be used with --output json.");
    }

    [Test]
    public async Task Upgrade_Version_PrintsTheVersion_WithoutUpgrading()
    {
        var before = await File.ReadAllTextAsync("test.csproj");

        CommandAppResult result = CreateApp().Run(["upgrade", "--version"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(await File.ReadAllTextAsync("test.csproj")).IsEqualTo(before);
        await Assert.That(result.Output).DoesNotContain("Upgrading");
    }

    [Test]
    public async Task Error_When_WrongOutputOptionIsGiven()
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
        });

        await Assert.That(() => app.Run(["check", "--output", "xml"])).ThrowsExactly<CommandRuntimeException>();
    }

    private static CommandAppTester CreateApp()
    {
        var app = TestHelper.CreateCommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<CheckCommand>("check");
            config.AddCommand<UpgradeCommand>("upgrade");
        });

        return app;
    }
}

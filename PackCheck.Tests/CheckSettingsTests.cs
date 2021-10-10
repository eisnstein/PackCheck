using PackCheck.Commands;
using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Xunit;
using Xunit.Abstractions;

namespace PackCheck.Tests
{
    public class CheckSettingsTests
    {
        private readonly ITestOutputHelper _output;

        public CheckSettingsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PathToCsProjFileIsNotSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<CheckCommand>("check");
            });

            CommandAppResult result = app.Run(new[] { "check" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<CheckSettings>(result.Settings);
            CheckSettings settings = result.Settings as CheckSettings;
            Assert.Null(settings!.PathToCsProjFile);
        }

        [Fact]
        public void PathToCsProjFileIsSetButNoValueGiven()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                // config.PropagateExceptions();
                config.AddCommand<CheckCommand>("check");
            });

            CommandAppResult result = app.Run(new[] { "check", "--csprojFile" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.StartsWith(
                "Error: Option 'csprojFile' is defined but no value has been provided.",
                result.Output
            );
        }

        [Fact]
        public void PathToCsProjFileIsSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                // config.PropagateExceptions();
                config.AddCommand<CheckCommand>("check");
            });

            CommandAppResult result = app.Run(new[] { "check", "--csprojFile", @".\example.csproj" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<CheckSettings>(result.Settings);
            CheckSettings settings = result.Settings as CheckSettings;
            Assert.NotNull(settings!.PathToCsProjFile);
            Assert.Equal(@".\example.csproj", settings!.PathToCsProjFile);
        }
    }
}

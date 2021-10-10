using PackCheck.Commands;
using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Xunit;

namespace PackCheck.Tests
{
    public class UpgradeSettingsTests
    {
        [Fact]
        public void DefaultTargetVersionIsSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[] { "upgrade" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.NotNull(settings!.Version);
            Assert.Equal("stable", settings!.Version);
        }

        [Fact]
        public void TargetVersionGetsSetToStable()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[] { "upgrade", "--version", "stable" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.NotNull(settings!.Version);
            Assert.Equal("stable", settings!.Version);
        }

        [Fact]
        public void TargetVersionGetsSetToLatest()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[] { "upgrade", "--version", "latest" });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.NotNull(settings!.Version);
            Assert.Equal("latest", settings!.Version);
        }

        [Fact]
        public void AllArgumentsWithoutDryRunGetSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[]
            {
                "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--version", "latest"
            });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.NotNull(settings!.PackageToUpgrade);
            Assert.Equal("awesome-package", settings!.PackageToUpgrade);
            Assert.NotNull(settings!.PathToCsProjFile);
            Assert.Equal("\\path-to-file", settings!.PathToCsProjFile);
            Assert.NotNull(settings!.Version);
            Assert.Equal("latest", settings!.Version);
            Assert.False(settings!.DryRun);
            Assert.False(settings!.Interactive);
        }

        [Fact]
        public void AllArgumentsWithDryRunGetSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[]
            {
                "upgrade", "awesome-package", "--csprojFile", "\\path-to-file", "--version", "latest", "--dry-run"
            });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.NotNull(settings!.PackageToUpgrade);
            Assert.Equal("awesome-package", settings!.PackageToUpgrade);
            Assert.NotNull(settings!.PathToCsProjFile);
            Assert.Equal("\\path-to-file", settings!.PathToCsProjFile);
            Assert.NotNull(settings!.Version);
            Assert.Equal("latest", settings!.Version);
            Assert.True(settings!.DryRun);
            Assert.False(settings!.Interactive);
        }

        [Fact]
        public void InteractiveIsNotSet()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[]
            {
                "upgrade"
            });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.False(settings!.Interactive);
        }

        [Fact]
        public void InteractiveIsSetByShortCode()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[]
            {
                "upgrade", "-i"
            });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.True(settings!.Interactive);
        }

        [Fact]
        public void InteractiveIsSetByLongCode()
        {
            var app = new CommandAppTester();
            app.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<UpgradeCommand>("upgrade");
            });

            CommandAppResult result = app.Run(new[]
            {
                "upgrade", "--interactive"
            });

            // No .csproj file is given or can be found, so we expect -1
            Assert.Equal(-1, result.ExitCode);
            Assert.NotNull(result.Settings);
            Assert.IsType<UpgradeSettings>(result.Settings);
            UpgradeSettings settings = result.Settings as UpgradeSettings;
            Assert.True(settings!.Interactive);
        }
    }
}

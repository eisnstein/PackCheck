using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings
{
    public sealed class UpgradeSettings : CommandSettings
    {
        [CommandArgument(0, "[package_name]")]
        [Description("Name of package to upgrade")]
        public string? PackageToUpgrade { get; init; }

        [CommandOption("--csprojFile <path>")]
        [Description(@"Path to *.csproj file. (default .\*.csproj)")]
        public string? PathToCsProjFile { get; init; }

        [CommandOption("--version <target_version>")]
        [Description("Upgrade version number to latest stable version (stable) or latest version (latest)")]
        [ValidateTargetVersion("Target version has to be 'stable' or 'latest'.")]
        public string Version { get; init; } = "stable";

        [CommandOption("--dry-run")]
        [DefaultValue(false)]
        [Description("Only show the result without actually changing the .csproj file")]
        public bool DryRun { get; init; }

        [CommandOption("-i|--interactive")]
        [DefaultValue(false)]
        [Description("Interactively decide for each package if it should be upgraded")]
        public bool Interactive { get; init; }
    }
}

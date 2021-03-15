using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings
{
    public class UpgradeSettings : CommandSettings
    {
        [CommandArgument(0, "[package_name]")]
        [Description("Name of package to upgrade")]
        public string? PackageToUpgrade { get; set; }

        [CommandOption("--csprojFile <path>")]
        [Description(@"Path to *.csproj file. (default .\*.csproj)")]
        public string? PathToCsProjFile { get; set; }

        [CommandOption("--version <target_version>")]
        [Description("Upgrade version number to latest stable version (stable) or latest version (latest)")]
        [ValidateTargetVersion]
        public string Version { get; set; } = "stable";

        [CommandOption("--dry-run")]
        [Description("Only show the result without actually changing the .csproj file")]
        public bool DryRun { get; set; } = false;
    }
}

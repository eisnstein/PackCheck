using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings;

public sealed class UpgradeSettings : CommonSettings
{
    [CommandArgument(0, "[Package_Name]")]
    [Description("Name of package to upgrade")]
    public string? PackageToUpgrade { get; set; }

    [CommandOption("-t|--target <Target_Version>")]
    [Description("Upgrade version number to latest stable version (stable) or latest version (latest)")]
    [ValidateTargetVersion("Target version has to be 'stable' or 'latest'.")]
    public string? Target { get; set; } = "stable";

    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    [Description("Only show the result without actually changing the .csproj file")]
    public bool DryRun { get; set; }

    [CommandOption("-i|--interactive")]
    [DefaultValue(false)]
    [Description("Interactively decide for each package if it should be upgraded")]
    public bool Interactive { get; set; }
}

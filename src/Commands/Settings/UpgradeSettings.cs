using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings;

public sealed class UpgradeSettings : CommonSettings
{
    [CommandArgument(0, "[Package_Name]")]
    [Description("Name of package to upgrade")]
    public string? PackageToUpgrade { get; set; }

    [CommandOption("--dry-run")]
    [DefaultValue(false)]
    [Description("Only show the result without actually changing the .csproj file")]
    public bool DryRun { get; set; }

    [CommandOption("-i|--interactive")]
    [DefaultValue(false)]
    [Description("Interactively decide for each package if it should be upgraded")]
    public bool Interactive { get; set; }
}

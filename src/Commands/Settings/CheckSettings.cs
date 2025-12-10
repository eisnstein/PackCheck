using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings;

public sealed class CheckSettings : CommonSettings
{
    [CommandOption("--format <Format>")]
    [Description("Format the output by the given value. Possible values: group")]
    [ValidateFormatValue("Format value has to be 'group'.")]
    public string? Format { get; set; }

    [CommandOption("--pre")]
    [Description("Show the latest version of packages (prerelease).")]
    public bool? ShowLatestVersion { get; set; }
}

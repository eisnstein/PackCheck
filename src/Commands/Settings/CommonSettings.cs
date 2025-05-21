using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings;

public class CommonSettings : CommandSettings
{
    [CommandOption("--csprojFile <Path>")]
    [Description(@"Path to *.csproj file. (default .\*.csproj)")]
    public string? PathToCsProjFile { get; set; }

    [CommandOption("--slnFile <Path>")]
    [Description(@"Path to *.sln file. (default .\*.sln)")]
    public string? PathToSlnFile { get; set; }

    [CommandOption("--slnxFile <Path>")]
    [Description(@"Path to *.slnx file. (default .\*.slnx)")]
    public string? PathToSlnxFile { get; set; }

    [CommandOption("--cpmFile <Path>")]
    [Description(@"Path to Directory.Packages.props file. (default .\Directory.Packages.props)")]
    public string? PathToCpmFile { get; set; }

    [CommandOption("-t|--target <Target_Version>")]
    [Description("Upgrade version number to latest stable version (stable) or latest version (latest)")]
    [ValidateTargetVersion("Target version has to be 'stable' or 'latest'.")]
    public string? Target { get; set; } = "stable";


    [CommandOption("-f|--filter <Package_Name>")]
    [Description("Include only packages matching the given name (can be used multiple times)")]
    public string[]? Filter { get; set; }

    [CommandOption("-x|--exclude <Package_Name>")]
    [Description("Exclude packages matching the given name (can be used multiple times)")]
    public string[]? Exclude { get; set; }

    [CommandOption("--format <Format>")]
    [Description("Format the output by the given value. Possible values: group")]
    [ValidateFormatValue("Format value has to be 'group'.")]
    public string? Format { get; set; }
}

using System.Collections.Generic;
using System.ComponentModel;
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

    [CommandOption("--cpmFile <Path>")]
    [Description(@"Path to Directory.Packages.props file. (default .\Directory.Packages.props)")]
    public string? PathToCpmFile { get; set; }

    [CommandOption("-f|--filter <Package_Name>")]
    [Description("Include only packages matching the given name (can be used multiple times)")]
    public string[]? Filter { get; set; }

    [CommandOption("-x|--exclude <Package_Name>")]
    [Description("Exclude packages matching the given name (can be used multiple times)")]
    public string[]? Exclude { get; set; }
}

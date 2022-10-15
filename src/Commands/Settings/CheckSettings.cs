using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings;

public sealed class CheckSettings : CommandSettings
{
    [CommandOption("--csprojFile <path>")]
    [Description(@"Path to *.csproj file. (default .\*.csproj)")]
    public string? PathToCsProjFile { get; init; }

    [CommandOption("--slnFile <path>")]
    [Description(@"Path to *.sln file. (default .\*.sln)")]
    public string? PathToSlnFile { get; init; }
}

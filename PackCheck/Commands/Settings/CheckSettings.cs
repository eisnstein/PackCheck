using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings
{
    public class CheckSettings : CommandSettings
    {
        [CommandOption("--csprojFile <path>")]
        [Description(@"Path to *.csproj file. (default .\*.csproj)")]
        public string? PathToCsProjFile { get; set; }
    }
}

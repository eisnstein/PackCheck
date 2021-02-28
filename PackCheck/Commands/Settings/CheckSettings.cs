using System.ComponentModel;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings
{
    public class CheckSettings : CommandSettings
    {
        [CommandOption("-p|--path <path>")]
        [Description("Path to .csproj file.")]
        public string PathToCsProjFile { get; set; } = string.Empty;
    }
}
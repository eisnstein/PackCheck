using System.ComponentModel;
using PackCheck.Commands.Validation;
using Spectre.Console.Cli;

namespace PackCheck.Commands.Settings
{
    public class CheckSettings : CommandSettings
    {
        [CommandArgument(0, "[package_name]")]
        [Description("Name of package to upgrade")]
        //[ValidatePackageName]
        public string? PackageToUpgrade { get; set; }

        [CommandOption("--csprojFile <path>")]
        [Description(@"Path to *.csproj file. (default .\*.csproj)")]
        public string? PathToCsProjFile { get; set; }

        [CommandOption("--target <target_version>")]
        [Description("Upgrade version number to latest stable version (stable) or latest version (latest)")]
        [ValidateTargetVersion]
        public string Target { get; set; } = "stable";

        [CommandOption("-u|--upgrade")]
        [Description("Upgrade all version numbers in your *.csproj file")]
        public bool Upgrade { get; set; } = false;
    }
}

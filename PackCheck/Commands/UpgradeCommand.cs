using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands
{
    public class UpgradeCommand : AsyncCommand<UpgradeSettings>
    {
        private readonly CsProjFileService _csProjFileService;
        private readonly NuGetPackagesService _nuGetPackagesService;
        private readonly List<Package> _packages;
        private string _pathToCsProjFile = string.Empty;

        public UpgradeCommand()
        {
            _csProjFileService = new CsProjFileService();
            _nuGetPackagesService = new NuGetPackagesService(new NuGetVersionService());
            _packages = new List<Package>();
        }

        public override async Task<int> ExecuteAsync(CommandContext context, UpgradeSettings settings)
        {
            try
            {
                _pathToCsProjFile = _csProjFileService.GetPathToCsProjFile(settings.PathToCsProjFile);
            }
            catch (CsProjFileNotFoundException ex)
            {
                AnsiConsole.Markup($"[dim]WARN:[/] [red]{ex.Message}[/]");
                return await Task.FromResult(-1);
            }

            Console.WriteLine($"Package name: {settings.PackageToUpgrade}");
            AnsiConsole.MarkupLine($"Upgrading to [silver]{settings.Version}[/] versions in [silver]{_pathToCsProjFile}[/]");

            await _nuGetPackagesService.GetPackagesDataFromCsProjFileAsync(_pathToCsProjFile, _packages);
            await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(_pathToCsProjFile, _packages);
            await _csProjFileService.UpgradePackageVersionsAsync(_pathToCsProjFile, _packages, settings.Version);

            AnsiConsole.MarkupLine(
                "[dim]INFO:[/] Run [blue]dotnet restore[/] to upgrade packages.");
            Console.WriteLine();

            return await Task.FromResult(0);
        }
    }
}

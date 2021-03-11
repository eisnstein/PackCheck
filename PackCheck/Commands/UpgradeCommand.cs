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

            await _nuGetPackagesService.GetPackagesDataFromCsProjFile(_pathToCsProjFile, _packages);
            await _nuGetPackagesService.GetPackagesDataFromNugetRepository(_pathToCsProjFile, _packages);

            return await Task.FromResult(0);
        }
    }
}

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
        private readonly SolutionFileService _solutionFileService;
        private readonly NuGetPackagesService _nuGetPackagesService;
        private string _pathToCsProjFile = string.Empty;

        public UpgradeCommand()
        {
            _csProjFileService = new CsProjFileService();
            _solutionFileService = new SolutionFileService();
            _nuGetPackagesService = new NuGetPackagesService(new NuGetVersionService());
        }

        public override async Task<int> ExecuteAsync(CommandContext context, UpgradeSettings settings)
        {
            Result? result = null;

            // Check if a path to a specific project is given
            if (!string.IsNullOrEmpty(settings.PathToCsProjFile))
            {
                result = await UpgradeProject(settings);
                if (result != Result.Success)
                {
                    return -1;
                }

                return 0;
            }

            // Check if we are in a solution or a path to a solution file is given
            if (_solutionFileService.HasSolution() || !string.IsNullOrEmpty(settings.PathToSlnFile))
            {
                var pathToSolutionFile = _solutionFileService.GetPathToSolutionFile(settings.PathToSlnFile);
                var projectDefinitions = _solutionFileService.GetProjectDefinitions(pathToSolutionFile);
                var projectCsProjFiles = _solutionFileService.ParseProjectDefinitions(projectDefinitions);

                foreach (var projectCsProjFile in projectCsProjFiles)
                {
                    settings.PathToCsProjFile = projectCsProjFile;

                    result = await UpgradeProject(settings);
                    if (result == Result.Error)
                    {
                        return -1;
                    }
                }

                return 0;
            }

            // No specific project file is given, no solution file given, also not in a solution
            // Check if a project file exists
            result = await UpgradeProject(settings);
            if (result == Result.Error)
            {
                return -1;
            }

            return 0;
        }

        private async Task<Result> UpgradeProject(UpgradeSettings settings)
        {
            // Get the path to the *.csproj file
            try
            {
                _pathToCsProjFile = _csProjFileService.GetPathToCsProjFile(settings.PathToCsProjFile);
            }
            catch (CsProjFileException ex)
            {
                AnsiConsole.Markup($"[dim]WARN:[/] [red]{ex.Message}[/]");
                return Result.Error;
            }

            AnsiConsole.MarkupLine($"Upgrading to [silver]{settings.Version}[/] versions in [silver]{_pathToCsProjFile}[/]");

            List<Package> packages = new();

            // Check that packages are defined in the *.csproj file
            await _nuGetPackagesService.GetPackagesDataFromCsProjFileAsync(_pathToCsProjFile, packages);
            if (packages.Count == 0)
            {
                AnsiConsole.MarkupLine($"Could not find any packages in [silver]{_pathToCsProjFile}[/]");
                return Result.Warning;
            }

            // If only a specific package should be upgraded,
            // ignore all the others
            if (!string.IsNullOrEmpty(settings.PackageToUpgrade))
            {
                packages.RemoveAll(p => p.PackageName != settings.PackageToUpgrade);
            }

            await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);
            await _csProjFileService.UpgradePackageVersionsAsync(_pathToCsProjFile, packages, settings);

            if (!settings.DryRun)
            {
                if (settings.Interactive)
                {
                    Console.WriteLine();
                }

                AnsiConsole.MarkupLine(
                    "[dim]INFO:[/] Run [blue]dotnet restore[/] to upgrade packages.");
            }

            Console.WriteLine();

            return Result.Success;
        }
    }
}

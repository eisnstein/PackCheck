using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Version = PackCheck.Data.Version;

namespace PackCheck.Commands;

public class UpgradeCommand : AsyncCommand<UpgradeSettings>
{
    private readonly CsProjFileService _csProjFileService;
    private readonly SolutionFileService _solutionFileService;
    private readonly NuGetPackagesService _nuGetPackagesService;
    private readonly PackageVersionHighlighterService _packageVersionHighlighterService;
    private string _pathToCsProjFile = string.Empty;

    public UpgradeCommand()
    {
        _csProjFileService = new CsProjFileService();
        _solutionFileService = new SolutionFileService();
        _nuGetPackagesService = new NuGetPackagesService(new NuGetVersionService());
        _packageVersionHighlighterService = new PackageVersionHighlighterService();
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

        AnsiConsole.MarkupLine($"Upgrading to [grey]{settings.Version}[/] versions in [grey]{_pathToCsProjFile}[/]");

        List<Package> packages = new();

        // Get information for all installed packages
        await _nuGetPackagesService.GetPackagesDataFromCsProjFileAsync(_pathToCsProjFile, packages);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"Could not find any packages in [grey]{_pathToCsProjFile}[/]");
            return Result.Warning;
        }

        // If only a specific package should be upgraded,
        // ignore all the others
        if (!string.IsNullOrEmpty(settings.PackageToUpgrade))
        {
            packages.RemoveAll(p => p.PackageName != settings.PackageToUpgrade);
        }

        // Fetch data for each package from nuget and store data on each package
        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        packages = PackagesService.PreparePackagesForUpgrade(packages, settings.Version);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]All packages are up to date.[/]");
            Console.WriteLine();
            return Result.Success;
        }

        if (settings.Interactive)
        {
            packages = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Package>()
                    .Title("Select which packages to upgrade:")
                    .NotRequired()
                    .PageSize(30)
                    .InstructionsText(
                        "[grey]<up>/<down> to select a package[/]" + Environment.NewLine +
                        "[grey]<space> to toggle a package[/]" + Environment.NewLine +
                        "[grey]<enter> to upgrade[/]")
                    .UseConverter(FormatSelectRow)
                    .AddChoices(packages)
            );

            if (packages.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No packages selected to upgrade.[/]");
                Console.WriteLine();
                return Result.Warning;
            }
        }

        await _csProjFileService.UpgradePackageVersionsAsync(_pathToCsProjFile, packages, settings.DryRun);

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

    private string FormatSelectRow(Package package)
    {
        var newVersion = package.UpgradeTo switch
        {
            Version.Latest => _packageVersionHighlighterService.HighlightLatestVersion(package),
            _ => _packageVersionHighlighterService.HighlightLatestStableVersion(package)
        };

        return $"{package.PackageName}: {package.CurrentVersion} -> {newVersion}";
    }
}

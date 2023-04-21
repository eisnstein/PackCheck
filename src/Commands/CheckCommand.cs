using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Spectre.Console;

namespace PackCheck.Commands;

public class CheckCommand : AsyncCommand<CheckSettings>
{
    private readonly CsProjFileService _csProjFileService;
    private readonly SolutionFileService _solutionFileService;
    private readonly NuGetApiService _nuGetApiService;
    private readonly NuGetPackagesService _nuGetPackagesService;
    private string _pathToCsProjFile = string.Empty;

    public CheckCommand()
    {
        _csProjFileService = new CsProjFileService();
        _solutionFileService = new SolutionFileService();
        _nuGetApiService = new NuGetApiService();
        _nuGetPackagesService = new NuGetPackagesService(_nuGetApiService);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CheckSettings settings)
    {
        await PackCheckService.CheckForNewPackCheckVersion(_nuGetApiService);

        Result? result = null;

        // Check if a path to a specific project is given
        if (!string.IsNullOrEmpty(settings.PathToCsProjFile))
        {
            result = await CheckProject(settings.PathToCsProjFile);
            if (result != Result.Success)
            {
                return -1;
            }

            PrintInfo();
            Console.WriteLine();

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
                result = await CheckProject(projectCsProjFile);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            PrintSolutionInfo();
            Console.WriteLine();

            return 0;
        }

        // No specific project file is given, no solution file given, also not in a solution
        // Check if a project file exists
        result = await CheckProject();
        if (result != Result.Success)
        {
            return -1;
        }

        PrintInfo();
        Console.WriteLine();

        return 0;
    }

    private async Task<Result> CheckProject(string? pathToCsProjFile = null)
    {
        List<Package> packages = new();

        try
        {
            _pathToCsProjFile = _csProjFileService.GetPathToCsProjFile(pathToCsProjFile);
        }
        catch (CsProjFileException ex)
        {
            AnsiConsole.MarkupLine($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Checking versions for [grey]{_pathToCsProjFile}[/]");

        await _nuGetPackagesService.GetPackagesDataFromCsProjFileAsync(_pathToCsProjFile, packages);

        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"Could not find any packages in [grey]{_pathToCsProjFile}[/]");
            return Result.Warning;
        }

        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        PrintTable(packages);
        Console.WriteLine();

        return Result.Success;
    }

    private void PrintTable(IReadOnlyList<Package> packages)
    {
        var service = new PackageVersionHighlighterService();
        var table = new Table();

        table.AddColumn("Package Name");
        table.AddColumn("Current Version");
        table.AddColumn("Latest Stable Version");
        table.AddColumn("Latest Version");

        foreach (Package p in packages)
        {
            table.AddRow(
                p.PackageName,
                p.CurrentVersion.ToString(),
                service.HighlightLatestStableVersion(p),
                service.HighlightLatestVersion(p)
            );
        }

        table.Columns[1].RightAligned();
        table.Columns[2].RightAligned();
        table.Columns[3].RightAligned();

        AnsiConsole.Write(table);
    }

    private void PrintInfo()
    {
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade[/] to upgrade the .csproj file with the latest stable versions.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade --version latest[/] to upgrade the .csproj file with the latest versions.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade <Package Name>[/] to upgrade only the specified package to the latest stable version.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade <Package Name> --version latest[/] to upgrade only the specified package to the latest version.");
    }

    private void PrintSolutionInfo()
    {
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade[/] to upgrade all .csproj files with the latest stable versions.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade --version latest[/] to upgrade all .csproj files with the latest versions.");
    }
}

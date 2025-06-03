using PackCheck.Commands.Settings;
using Spectre.Console.Cli;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Spectre.Console;

namespace PackCheck.Commands;

public class CheckCommand : AsyncCommand<CheckSettings>
{
    private readonly NuGetApiService _nuGetApiService;
    private readonly NuGetPackagesService _nuGetPackagesService;

    public CheckCommand()
    {
        _nuGetApiService = new NuGetApiService();
        _nuGetPackagesService = new NuGetPackagesService(_nuGetApiService);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CheckSettings settings)
    {
        await PackCheckService.CheckForNewPackCheckVersion(_nuGetApiService);

        Result? result = null;
        string? pathToConfigFile = null;
        Config? config = null;

        try
        {
            (pathToConfigFile, config) = PackCheckConfigService.GetConfig();
        }
        catch (JsonException e)
        {
            AnsiConsole.MarkupLine(
                $"[dim]WARN:[/] Your .packcheckrc[.json] configuration file seems to be invalid: {e.Message}");
            return -1;
        }

        if (config is not null)
        {
            AnsiConsole.MarkupLine($"Reading config from [grey]{pathToConfigFile}[/]");
        }

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        // Check if a path to a specific project is given
        if (!string.IsNullOrEmpty(settings.PathToCsProjFile))
        {
            result = await CheckProject(settings.PathToCsProjFile, settings);
            if (result != Result.Success)
            {
                return -1;
            }

            PrintInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a path to a solution file is given
        if (!string.IsNullOrEmpty(settings.PathToSlnFile))
        {
            var pathToSolutionFile = SolutionFileService.GetPathToSolutionFile(settings.PathToSlnFile);
            var projectDefinitions = SolutionFileService.GetProjectDefinitions(pathToSolutionFile);
            var projectCsProjFiles = SolutionFileService.ParseProjectDefinitions(projectDefinitions);

            foreach (var projectCsProjFile in projectCsProjFiles)
            {
                result = await CheckProject(projectCsProjFile, settings);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            PrintSolutionInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a path to a solution X file is given
        if (!string.IsNullOrEmpty(settings.PathToSlnxFile))
        {
            var pathToSolutionXFile = SolutionXFileService.GetPathToSolutionXFile(settings.PathToSlnxFile);
            var projectCsProjFiles = SolutionXFileService.ParseProjectDefinitions(pathToSolutionXFile);

            foreach (var projectCsProjFile in projectCsProjFiles)
            {
                result = await CheckProject(projectCsProjFile, settings);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            PrintSolutionInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a path to a Central Packages Mgmt file is given
        if (!string.IsNullOrEmpty(settings.PathToCpmFile))
        {
            var pathToCpmFile = CentralPackageMgmtService.GetPathToCpmFile(settings.PathToCpmFile);

            result = await CheckCpm(pathToCpmFile, settings);
            if (result != Result.Success)
            {
                return -1;
            }

            PrintInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if Central Package Management is used
        if (CentralPackageMgmtService.HasCentralPackageMgmt())
        {
            result = await CheckCpm(settings: settings);
            if (result != Result.Success)
            {
                return -1;
            }

            PrintInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a solution file exists
        if (SolutionFileService.HasSolution())
        {
            var pathToSolutionFile = SolutionFileService.GetPathToSolutionFile(settings.PathToSlnFile);
            var projectDefinitions = SolutionFileService.GetProjectDefinitions(pathToSolutionFile);
            var projectCsProjFiles = SolutionFileService.ParseProjectDefinitions(projectDefinitions);

            foreach (var projectCsProjFile in projectCsProjFiles)
            {
                result = await CheckProject(projectCsProjFile, settings);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            PrintSolutionInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a solution X file exists
        if (SolutionXFileService.HasSolutionX())
        {
            var pathToSolutionXFile = SolutionXFileService.GetPathToSolutionXFile(settings.PathToSlnxFile);
            var projectCsProjFiles = SolutionXFileService.ParseProjectDefinitions(pathToSolutionXFile);

            foreach (var projectCsProjFile in projectCsProjFiles)
            {
                result = await CheckProject(projectCsProjFile, settings);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            PrintSolutionInfo();
            Console.WriteLine();

            return 0;
        }

        // Check if a project file exists
        result = await CheckProject(settings: settings);
        if (result != Result.Success)
        {
            return -1;
        }

        PrintInfo();
        Console.WriteLine();

        return 0;
    }

    private async Task<Result> CheckProject(string? pathToCsProjFile = null, CheckSettings? settings = null)
    {
        try
        {
            pathToCsProjFile = CsProjFileService.GetPathToCsProjFile(pathToCsProjFile);
        }
        catch (CsProjFileException ex)
        {
            AnsiConsole.MarkupLine($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Checking versions for [grey]{pathToCsProjFile}[/]");

        List<Package> packages = await CsProjFileService.GetPackagesDataFromCsProjFileAsync(pathToCsProjFile);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"Could not find any packages in [grey]{pathToCsProjFile}[/]");
            return Result.Warning;
        }

        packages = PackagesService.ApplySettings(packages, settings);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"No packages to check. Check your 'filter' or 'exclude' in settings/config.");
            return Result.Warning;
        }

        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        packages = PackagesService.CalculateUpgradeType(packages, settings);

        PrintResult(packages, settings);
        Console.WriteLine();

        return Result.Success;
    }

    private async Task<Result> CheckCpm(string? pathToCpmFile = null, CheckSettings? settings = null)
    {
        try
        {
            pathToCpmFile = CentralPackageMgmtService.GetPathToCpmFile(pathToCpmFile);
        }
        catch (CpmFileException ex)
        {
            AnsiConsole.MarkupLine($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Checking versions for [grey]{pathToCpmFile}[/]");

        List<Package> packages = await CentralPackageMgmtService.GetPackagesDataFromCpmFileAsync(pathToCpmFile);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"Could not find any packages in [grey]{pathToCpmFile}[/]");
            return Result.Warning;
        }

        packages = PackagesService.ApplySettings(packages, settings);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"No packages to check. Check your 'filter' or 'exclude' in settings/config.");
            return Result.Warning;
        }

        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        packages = PackagesService.CalculateUpgradeType(packages, settings);

        PrintResult(packages, settings);
        Console.WriteLine();

        return Result.Success;
    }

    private void PrintResult(List<Package> packages, CheckSettings? settings)
    {
        if (settings is { Format: "group" })
        {
            var query = packages.GroupBy(p => p.UpgradeType);

            var patch = query.FirstOrDefault(group => group.Key == EUpgradeType.Patch);
            var minor = query.FirstOrDefault(group => group.Key == EUpgradeType.Minor);
            var major = query.FirstOrDefault(group => group.Key == EUpgradeType.Major);

            if (patch is not null)
            {
                var patchPackages = patch.ToList();
                AnsiConsole.MarkupLine( "[green]Patch[/] [dim]Backwards compatible - bug fixes[/]");

                PrintTable(patchPackages);
                Console.WriteLine();
            }

            if (minor is not null)
            {
                var minorPackages = minor.ToList();
                AnsiConsole.MarkupLine( "[yellow]Minor[/] [dim]Backwards compatible - new features[/]");

                PrintTable(minorPackages);
                Console.WriteLine();
            }

            if (major is not null)
            {
                var majorPackages = major.ToList();
                AnsiConsole.MarkupLine( "[red]Major[/] [dim]Possibly breaking changes - check Changelog[/]");

                PrintTable(majorPackages);
                Console.WriteLine();
            }

            return;
        }

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
                PackageVersionHighlighterService.HighlightLatestStableVersion(p),
                PackageVersionHighlighterService.HighlightLatestVersion(p)
            );
        }

        table.Columns[1].RightAligned();
        table.Columns[2].RightAligned();
        table.Columns[3].RightAligned();

        AnsiConsole.Write(table);
    }

    private void PrintTable(List<Package> packages)
    {
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
                PackageVersionHighlighterService.HighlightLatestStableVersion(p),
                PackageVersionHighlighterService.HighlightLatestVersion(p)
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
            "[dim]INFO:[/] Run [blue]packcheck upgrade --target latest[/] to upgrade the .csproj file with the latest versions.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade <Package Name>[/] to upgrade only the specified package to the latest stable version.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade <Package Name> --target latest[/] to upgrade only the specified package to the latest version.");
    }

    private void PrintSolutionInfo()
    {
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade[/] to upgrade all .csproj files with the latest stable versions.");
        AnsiConsole.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade --target latest[/] to upgrade all .csproj files with the latest versions.");
    }
}

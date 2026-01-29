using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using RouteCheck.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace PackCheck.Commands;

public class CheckCommand : AsyncCommand<CheckSettings>
{
    private readonly IAnsiConsole _console;
    private readonly NuGetApiService _nuGetApiService;
    private readonly NuGetPackagesService _nuGetPackagesService;

    public CheckCommand(IAnsiConsole console)
    {
        _console = console;
        _nuGetApiService = new NuGetApiService();
        _nuGetPackagesService = new NuGetPackagesService(_nuGetApiService);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CheckSettings settings, CancellationToken _cancellationToken)
    {
        if (settings.Version == true)
        {
            OutputService.PrintVersionInfo(_console);
            return 0;
        }

        await PackCheckService.CheckForNewPackCheckVersion(_nuGetApiService);

        Result? result;
        string? pathToConfigFile;
        Config? config;

        try
        {
            (pathToConfigFile, config) = PackCheckConfigService.GetConfig();
        }
        catch (JsonException e)
        {
            _console.MarkupLine(
                $"[dim]WARN:[/] Your .packcheckrc[.json] configuration file seems to be invalid: {e.Message}");
            return -1;
        }

        if (config is not null)
        {
            _console.MarkupLine($"Reading config from [grey]{pathToConfigFile}[/]");
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

            OutputService.PrintInfo(_console);
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

            OutputService.PrintSolutionInfo(_console);
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

            OutputService.PrintSolutionInfo(_console);
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

            OutputService.PrintInfo(_console);
            Console.WriteLine();

            return 0;
        }

        // Check if a path to a file-based app file is given
        if (!string.IsNullOrEmpty(settings.PathToFbaFile))
        {
            var pathToFileBasedAppFile = FileBasedAppService.GetPathToFileBasedAppFile(settings.PathToFbaFile);

            result = await CheckFba(pathToFileBasedAppFile, settings);
            if (result != Result.Success)
            {
                return -1;
            }

            OutputService.PrintInfo(_console);
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

            OutputService.PrintInfo(_console);
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

            OutputService.PrintSolutionInfo(_console);
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

            OutputService.PrintSolutionInfo(_console);
            Console.WriteLine();

            return 0;
        }

        // Check if a project file exists
        result = await CheckProject(settings: settings);
        if (result != Result.Success)
        {
            return -1;
        }

        OutputService.PrintInfo(_console);
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
        packages = PackagesService.PreparePackagesForOutput(packages);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"All packages are up to date.");
            return Result.Success;
        }

        OutputService.PrintResult(_console, packages, settings);
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
        packages = PackagesService.PreparePackagesForOutput(packages);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"All packages are up to date.");
            return Result.Success;
        }

        OutputService.PrintResult(_console, packages, settings);
        Console.WriteLine();

        return Result.Success;
    }

    private async Task<Result> CheckFba(string pathToFbaFile, CheckSettings? settings = null)
    {
        try
        {
            pathToFbaFile = FileBasedAppService.GetPathToFileBasedAppFile(pathToFbaFile);
        }
        catch (FileBasedAppFileException ex)
        {
            AnsiConsole.MarkupLine($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Checking versions for [grey]{pathToFbaFile}[/]");

        List<Package> packages = FileBasedAppService.GetPackagesDataFromFbaFile(pathToFbaFile);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"Could not find any packages in [grey]{pathToFbaFile}[/]");
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
        packages = PackagesService.PreparePackagesForOutput(packages);
        if (packages.Count == 0)
        {
            AnsiConsole.MarkupLine($"All packages are up to date.");
            return Result.Success;
        }

        OutputService.PrintResult(_console, packages, settings);
        Console.WriteLine();

        return Result.Success;
    }

}
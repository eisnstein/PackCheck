using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands;

public class UpgradeCommand : AsyncCommand<UpgradeSettings>
{
    private readonly NuGetPackagesService _nuGetPackagesService = new(new NuGetApiService());

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

        // Check if a path to a solution file is given
        if (!string.IsNullOrEmpty(settings.PathToSlnFile))
        {
            var pathToSolutionFile = SolutionFileService.GetPathToSolutionFile(settings.PathToSlnFile);
            var projectDefinitions = SolutionFileService.GetProjectDefinitions(pathToSolutionFile);
            var projectCsProjFiles = SolutionFileService.ParseProjectDefinitions(projectDefinitions);

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

        // Check if a path to a solution X file is given
        if (!string.IsNullOrEmpty(settings.PathToSlnxFile))
        {
            var pathToSolutionXFile = SolutionXFileService.GetPathToSolutionXFile(settings.PathToSlnxFile);
            var projectCsProjFiles = SolutionXFileService.ParseProjectDefinitions(pathToSolutionXFile);

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

        // Check if a path to a Central Packages Mgmt file is given
        if (!string.IsNullOrEmpty(settings.PathToCpmFile))
        {
            result = await UpgradeCpm(settings);
            if (result != Result.Success)
            {
                return -1;
            }

            return 0;
        }

        // Check if Central Package Management is used
        if (CentralPackageMgmtService.HasCentralPackageMgmt())
        {
            result = await UpgradeCpm(settings);
            if (result != Result.Success)
            {
                return -1;
            }

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
                settings.PathToCsProjFile = projectCsProjFile;

                result = await UpgradeProject(settings);
                if (result == Result.Error)
                {
                    return -1;
                }
            }

            return 0;
        }

        // Check if a solution X file exists
        if (SolutionXFileService.HasSolutionX())
        {
            var pathToSolutionXFile = SolutionXFileService.GetPathToSolutionXFile(settings.PathToSlnxFile);
            var projectCsProjFiles = SolutionXFileService.ParseProjectDefinitions(pathToSolutionXFile);

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
        var pathToCsProjFile = "";
        string? pathToConfigFile = null;
        Config? config = null;

        try
        {
            (pathToConfigFile, config) = PackCheckConfigService.GetConfig();
        }
        catch (JsonException e)
        {
            AnsiConsole.MarkupLine(
                $"[dim]WARN:[/] Your .packcheckrc.json configuration file seems to be invalid: ${e.Message}");
            return Result.Error;
        }

        if (config is not null)
        {
            AnsiConsole.MarkupLine($"Reading config from [grey]{pathToConfigFile}[/]");
        }

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        // Get the path to the *.csproj file
        try
        {
            pathToCsProjFile = CsProjFileService.GetPathToCsProjFile(settings.PathToCsProjFile);
        }
        catch (CsProjFileException ex)
        {
            AnsiConsole.Markup($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Upgrading to [grey]{settings.Target}[/] versions in [grey]{pathToCsProjFile}[/]");

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

        // If only a specific package should be upgraded,
        // ignore all the others
        if (!string.IsNullOrEmpty(settings.PackageToUpgrade))
        {
            packages.RemoveAll(p => p.PackageName != settings.PackageToUpgrade);
        }

        // Fetch data for each package from nuget and store data on each package
        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        packages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target!);
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

        await CsProjFileService.UpgradePackageVersionsAsync(pathToCsProjFile, packages, settings.DryRun);

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

    private async Task<Result> UpgradeCpm(UpgradeSettings settings)
    {
        var pathToCpmFile = "";
        string? pathToConfigFile = null;
        Config? config = null;

        try
        {
            (pathToConfigFile, config) = PackCheckConfigService.GetConfig();
        }
        catch (JsonException e)
        {
            AnsiConsole.MarkupLine(
                $"[dim]WARN:[/] Your .packcheckrc.json configuration file seems to be invalid: ${e.Message}");
            return Result.Error;
        }

        if (config is not null)
        {
            AnsiConsole.MarkupLine($"Reading config from [grey]{pathToConfigFile}[/]");
        }

        settings = SettingsService.CombineSettingsWithConfig(settings, config);

        // Get the path to the *.csproj file
        try
        {
            pathToCpmFile = CentralPackageMgmtService.GetPathToCpmFile(settings.PathToCpmFile);
        }
        catch (CpmFileException ex)
        {
            AnsiConsole.Markup($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return Result.Error;
        }

        AnsiConsole.MarkupLine($"Upgrading to [grey]{settings.Target}[/] versions in [grey]{pathToCpmFile}[/]");

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

        // If only a specific package should be upgraded,
        // ignore all the others
        if (!string.IsNullOrEmpty(settings.PackageToUpgrade))
        {
            packages.RemoveAll(p => p.PackageName != settings.PackageToUpgrade);
        }

        // Fetch data for each package from nuget and store data on each package
        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        packages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target!);
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

        await CentralPackageMgmtService.UpgradePackageVersionsAsync(pathToCpmFile, packages, settings.DryRun);

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
            Target.Latest => PackageVersionHighlighterService.HighlightLatestVersion(package),
            _ => PackageVersionHighlighterService.HighlightLatestStableVersion(package)
        };

        return $"{package.PackageName}: {package.CurrentVersion} -> {newVersion}";
    }
}

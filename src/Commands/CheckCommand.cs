using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.PackageFiles;
using PackCheck.Services;
using Spectre.Console;

namespace PackCheck.Commands;

public class CheckCommand(IAnsiConsole console, INuGetApiService nuGetApiService)
    : PackageCommand<CheckSettings>(console, nuGetApiService)
{
    protected override async Task<int?> BeforeRunAsync(CheckSettings settings)
    {
        if (settings.Version == true)
        {
            OutputService.PrintVersionInfo(Output);
            return 0;
        }

        await PackCheckService.CheckForNewPackCheckVersion(NuGetApiService, Output);

        return null;
    }

    protected override string DescribeFile(IPackageFile file, CheckSettings settings)
    {
        return $"Checking versions for [grey]{file.Path.EscapeMarkup()}[/]";
    }

    protected override Task<Result> ProcessPackagesAsync(
        IPackageFile file, List<Package> packages, PackCheckContext<CheckSettings> context)
    {
        packages = PackagesService.CalculateUpgradeType(packages, context.Settings);
        packages = PackagesService.PreparePackagesForOutput(packages);
        if (packages.Count == 0)
        {
            Output.MarkupLine("All packages are up to date.");
            return Task.FromResult(Result.Success);
        }

        OutputService.PrintResult(Output, packages, context.Settings);
        Output.WriteLine();

        return Task.FromResult(Result.Success);
    }

    protected override void AfterRun(PackCheckContext<CheckSettings> context)
    {
        if (context.IsSolution)
        {
            OutputService.PrintSolutionInfo(Output);
        }
        else
        {
            OutputService.PrintInfo(Output);
        }

        Output.WriteLine();
    }
}

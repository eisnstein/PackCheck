using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.PackageFiles;
using PackCheck.Services;
using Spectre.Console;

namespace PackCheck.Commands;

public class UpgradeCommand(IAnsiConsole console, INuGetApiService nuGetApiService)
    : PackageCommand<UpgradeSettings>(console, nuGetApiService)
{
    protected override string? ValidateSettings(UpgradeSettings settings)
    {
        // The prompt would end up in the document
        return settings.Interactive && IsStructuredOutput
            ? $"--interactive cannot be used with --output {settings.Output}."
            : null;
    }

    protected override string DescribeFile(IPackageFile file, UpgradeSettings settings)
    {
        return $"Upgrading to [grey]{settings.Target.EscapeMarkup()}[/] versions in [grey]{file.Path.EscapeMarkup()}[/]";
    }

    protected override List<Package> SelectPackages(List<Package> packages, UpgradeSettings settings)
    {
        // If only a specific package should be upgraded,
        // ignore all the others
        if (!string.IsNullOrEmpty(settings.PackageToUpgrade))
        {
            packages.RemoveAll(p => !string.Equals(p.PackageName, settings.PackageToUpgrade, StringComparison.OrdinalIgnoreCase));
        }

        return packages;
    }

    protected override async Task<Result> ProcessPackagesAsync(
        IPackageFile file, List<Package> packages, PackCheckContext<UpgradeSettings> context)
    {
        var settings = context.Settings;

        packages = PackagesService.PreparePackagesForUpgrade(packages, settings.Target!);

        if (IsStructuredOutput)
        {
            // A dry run would print the changed file, the document shows what would change instead
            if (!settings.DryRun && packages.Count > 0)
            {
                await file.WriteVersionsAsync(packages, dryRun: false);
            }

            Report(file, packages);
            return Result.Success;
        }

        if (packages.Count == 0)
        {
            Output.MarkupLine("[green]All packages are up to date.[/]");
            Output.WriteLine();
            return Result.Success;
        }

        if (settings.Interactive)
        {
            packages = Output.Prompt(
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
                Output.MarkupLine("[grey]No packages selected to upgrade.[/]");
                Output.WriteLine();
                return Result.Warning;
            }
        }

        await file.WriteVersionsAsync(packages, settings.DryRun);

        if (!settings.DryRun)
        {
            if (settings.Interactive)
            {
                Output.WriteLine();
            }

            Output.MarkupLine("[dim]INFO:[/] Run [blue]dotnet restore[/] to upgrade packages.");
        }

        Output.WriteLine();

        return Result.Success;
    }

    protected override void WriteStructuredOutput(
        TextWriter writer, IReadOnlyList<FileReport> files, UpgradeSettings settings, string? error)
    {
        StructuredOutputService.WriteUpgrade(writer, files, settings, error);
    }

    private static string FormatSelectRow(Package package)
    {
        var newVersion = package.UpgradeTo switch
        {
            Target.Latest => PackageVersionHighlighterService.HighlightLatestVersion(package),
            _ => PackageVersionHighlighterService.HighlightLatestStableVersion(package)
        };

        return $"{package.PackageName}: {package.CurrentVersion} -> {newVersion}";
    }
}

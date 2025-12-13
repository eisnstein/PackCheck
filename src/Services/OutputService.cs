using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Services;
using Spectre.Console;

namespace RouteCheck.Services;

public static class OutputService
{
    public static void PrintResult(IAnsiConsole console, List<Package> packages, CheckSettings? settings)
    {
        if (settings is { Format: "group" })
        {
            var returnEarly = false;
            var query = packages.GroupBy(p => p.UpgradeType);

            var patch = query.FirstOrDefault(group => group.Key == EUpgradeType.Patch);
            var minor = query.FirstOrDefault(group => group.Key == EUpgradeType.Minor);
            var major = query.FirstOrDefault(group => group.Key == EUpgradeType.Major);

            if (patch is not null)
            {
                var patchPackages = patch.ToList();
                AnsiConsole.MarkupLine("[green]Patch[/] [dim]Backwards compatible - bug fixes[/]");

                PrintTable(console, patchPackages, settings);
                Console.WriteLine();
                returnEarly = true;
            }

            if (minor is not null)
            {
                var minorPackages = minor.ToList();
                AnsiConsole.MarkupLine("[yellow]Minor[/] [dim]Backwards compatible - new features[/]");

                PrintTable(console, minorPackages, settings);
                Console.WriteLine();
                returnEarly = true;
            }

            if (major is not null)
            {
                var majorPackages = major.ToList();
                AnsiConsole.MarkupLine("[red]Major[/] [dim]Possibly breaking changes - check Changelog[/]");

                PrintTable(console, majorPackages, settings);
                Console.WriteLine();
                returnEarly = true;
            }

            // If we printed something, we return early to avoid printing the table again below. If nothing was printed,
            // it means there are no updates available from the current to the stable version. So we print the full table below
            // to see potential pre-release versions.
            if (returnEarly)
            {
                return;
            }
        }

        PrintTable(console, packages, settings);
    }

    private static void PrintTable(IAnsiConsole console, List<Package> packages, CheckSettings? settings = null)
    {
        var table = new Table();

        table.AddColumn("Package Name");
        table.AddColumn("Current");
        table.AddColumn("Latest Stable");

        if (settings?.Pre == true)
        {
            table.AddColumn("Latest");
        }

        foreach (Package p in packages)
        {
            if (settings?.Pre == true)
            {
                table.AddRow(
                    p.PackageName,
                    p.CurrentVersion.ToString(),
                    PackageVersionHighlighterService.HighlightLatestStableVersion(p),
                    PackageVersionHighlighterService.HighlightLatestVersion(p)
                );
            }
            else
            {
                table.AddRow(
                    p.PackageName,
                    p.CurrentVersion.ToString(),
                    PackageVersionHighlighterService.HighlightLatestStableVersion(p)
                );
            }
        }

        table.Columns[1].RightAligned();
        table.Columns[2].RightAligned();

        if (settings?.Pre == true)
        {
            table.Columns[3].RightAligned();
        }

        console.Write(table);
    }

    public static void PrintInfo(IAnsiConsole console)
    {
        console.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade[/] to upgrade to the latest stable versions.");
        console.MarkupLine("[dim]INFO:[/] Run [blue]packcheck --help[/] for more options.");
    }

    public static void PrintSolutionInfo(IAnsiConsole console)
    {
        console.MarkupLine(
            "[dim]INFO:[/] Run [blue]packcheck upgrade[/] to upgrade all .csproj files with the latest stable versions.");
        console.MarkupLine("[dim]INFO:[/] Run [blue]packcheck --help[/] for more options.");
    }
}
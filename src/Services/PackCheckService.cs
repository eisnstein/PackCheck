using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;
using NuGet.Versioning;
using Spectre.Console;

namespace PackCheck.Services;

public static class PackCheckService
{
    public static async Task CheckForNewPackCheckVersion(NuGetApiService nuGetApiService)
    {
        IEnumerable<NuGetVersion> result = await nuGetApiService.GetPackageVersions("PackCheck");
        ImmutableList<NuGetVersion> versions = result.ToImmutableList();

        if (versions is { Count: 0 })
        {
            return;
        }

        NuGetVersion? latestVersion = NuGetVersionService.GetLatestStableVersion(versions);
        if (latestVersion is null)
        {
            return;
        }

        var assembly = typeof(PackagesService).Assembly;
        var currentVersionStr = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        if (currentVersionStr is null)
        {
            return;
        }

        var currentVersion = NuGetVersion.Parse(currentVersionStr);

        // If there is a newer version available, show information
        if (latestVersion > currentVersion)
        {
            AnsiConsole.MarkupLine($"[dim]INFO:[/] A new version of PackCheck is available: {currentVersion} -> {latestVersion}");
            AnsiConsole.MarkupLine($"[dim]INFO:[/] Changelog: [link]https://github.com/eisnstein/PackCheck/blob/main/CHANGELOG.md[/]");
            AnsiConsole.MarkupLine("[dim]INFO:[/] Run [blue]dotnet tool update --global PackCheck[/] to update");
            Console.WriteLine();
        }
    }
}

using System.Collections.Immutable;
using System.Reflection;
using NuGet.Versioning;
using Spectre.Console;

namespace PackCheck.Services;

public static class PackCheckService
{
    public static async Task CheckForNewPackCheckVersion(INuGetApiService nuGetApiService, IAnsiConsole console)
    {
        IEnumerable<NuGetVersion> result;
        try
        {
            result = await nuGetApiService.GetPackageVersions("PackCheck", CancellationToken.None);
        }
        catch (Exception)
        {
            // This check is optional, so it must never stop the actual command,
            // e.g. when offline or behind a proxy.
            return;
        }

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
        if (!NuGetVersion.TryParse(currentVersionStr, out var currentVersion))
        {
            return;
        }

        // If there is a newer version available, show information
        if (latestVersion > currentVersion)
        {
            console.MarkupLine($"[dim]INFO:[/] A new version of PackCheck is available: {currentVersion} -> {latestVersion}");
            console.MarkupLine($"[dim]INFO:[/] Changelog: [link]https://github.com/eisnstein/PackCheck/blob/main/CHANGELOG.md[/]");
            console.MarkupLine("[dim]INFO:[/] Run [blue]dotnet tool update --global PackCheck[/] to update");
            console.WriteLine();
        }
    }
}

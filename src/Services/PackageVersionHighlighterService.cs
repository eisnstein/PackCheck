using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Services;

public static class PackageVersionHighlighterService
{
    public static string HighlightLatestStableVersion(Package p)
    {
        // For a prerelease without a newer stable version this is the latest prerelease.
        var stableTargetVersion = PackagesService.GetStableTargetVersion(p);
        if (stableTargetVersion is null)
        {
            return "-";
        }

        return p.CurrentVersion == stableTargetVersion
            ? p.CurrentVersion.ToString()
            : HighlightVersion(p.CurrentVersion, stableTargetVersion);
    }

    public static string HighlightLatestVersion(Package p)
    {
        if (p.LatestVersion is null)
        {
            return "-";
        }

        return p.CurrentVersion == p.LatestVersion
            ? p.CurrentVersion.ToString()
            : HighlightVersion(p.CurrentVersion, p.LatestVersion);
    }

    private static string HighlightVersion(NuGetVersion current, NuGetVersion newer)
    {
        // If the newer major version is greater or lower than the current major
        // version we have a breaking change and can return all red.
        if (newer.Major != current.Major)
        {
            return $"[red]{newer.ToNormalizedString()}[/]";
        }

        // NuGet versions can have a fourth part (e.g. 4.0.104.11), which is
        // part of the normalized version whenever it is not 0.
        var revision = newer.Revision > 0 ? $".{newer.Revision}" : string.Empty;
        var release = newer.IsPrerelease ? $"-{newer.Release}" : string.Empty;

        if (newer.Minor > current.Minor)
        {
            return $"{newer.Major}.[yellow]{newer.Minor}.{newer.Patch}{revision}{release}[/]";
        }

        if (newer.Patch > current.Patch)
        {
            return $"{newer.Major}.{newer.Minor}.[green]{newer.Patch}{revision}{release}[/]";
        }

        if (newer.Revision > current.Revision)
        {
            return $"{newer.Major}.{newer.Minor}.{newer.Patch}.[green]{newer.Revision}{release}[/]";
        }

        var numbers = $"{newer.Major}.{newer.Minor}.{newer.Patch}{revision}";

        if (newer.IsPrerelease)
        {
            return $"{numbers}[green]{release}[/]";
        }

        return current.IsPrerelease
            ? $"[green]{numbers}[/]"
            : numbers;
    }
}

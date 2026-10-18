using System.Text;
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
            return $"[red]{newer.ToString()}[/]";
        }

        var highlightedVersion = new StringBuilder();
        highlightedVersion
            .Append(newer.Major)
            .Append('.');

        if (newer.Minor > current.Minor)
        {
            highlightedVersion
                .Append("[yellow]")
                .Append(newer.Minor)
                .Append('.')
                .Append(newer.Patch);

            if (newer.IsPrerelease)
            {
                highlightedVersion
                    .Append('-')
                    .Append(newer.Release);
            }

            highlightedVersion
                .Append("[/]");

            return highlightedVersion.ToString();
        }

        if (newer.Patch > current.Patch)
        {
            highlightedVersion
                .Append(newer.Minor)
                .Append('.')
                .Append("[green]")
                .Append(newer.Patch);

            if (newer.IsPrerelease)
            {
                highlightedVersion
                    .Append('-')
                    .Append(newer.Release);
            }

            highlightedVersion
                .Append("[/]");

            return highlightedVersion.ToString();
        }

        highlightedVersion
            .Append(newer.Minor)
            .Append('.')
            .Append(newer.Patch);

        if (newer.IsPrerelease)
        {
            highlightedVersion
                .Append("[green]")
                .Append('-')
                .Append(newer.Release)
                .Append("[/]");
        }
        else if (current.IsPrerelease)
        {
            highlightedVersion
                .Insert(0, "[green]")
                .Append("[/]");
        }

        return highlightedVersion.ToString();
    }
}

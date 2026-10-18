using NuGet.Versioning;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using System.Text.RegularExpressions;

namespace PackCheck.Services;

public static class PackagesService
{
    public static List<Package> ApplySettings(List<Package> packages, CommonSettings? settings = null)
    {
        if (settings is null)
        {
            return packages;
        }

        if (settings is { Filter: { Length: > 0 } })
        {
            packages = packages.Where(p => settings.Filter.Any(filterPattern => MatchesPackageName(p.PackageName, filterPattern))).ToList();
        }

        if (settings is { Exclude: { Length: > 0 } })
        {
            packages = packages.Where(p => !settings.Exclude.Any(excludePattern => MatchesPackageName(p.PackageName, excludePattern))).ToList();
        }

        return packages;
    }

    private static bool MatchesPackageName(string packageName, string pattern)
    {
        if (!pattern.Contains('*') && !pattern.Contains('%'))
        {
            return packageName == pattern;
        }

        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("%", ".*") + "$";
        return Regex.IsMatch(packageName, regexPattern);
    }

    /// <summary>
    /// The version the "stable" target upgrades to, shown in the "Latest Stable" column.
    /// - Current version is stable: the latest stable version.
    /// - Current version is a prerelease: the latest stable version if it is newer
    ///   (e.g. 3.0.0-rc.1 -> 3.0.0), otherwise the latest prerelease, so packages
    ///   without a newer stable release stay on the prerelease lane.
    /// </summary>
    public static NuGetVersion? GetStableTargetVersion(Package p)
    {
        if (!p.CurrentVersion.IsPrerelease)
        {
            return p.LatestStableVersion;
        }

        if (p.LatestStableVersion is not null && p.LatestStableVersion > p.CurrentVersion)
        {
            return p.LatestStableVersion;
        }

        return p.LatestPrereleaseVersion ?? p.LatestStableVersion;
    }

    public static List<Package> CalculateUpgradeType(List<Package> packages, CheckSettings? settings)
    {
        if (settings is null)
        {
            return packages;
        }

        foreach (var p in packages)
        {
            var currentVersion = p.CurrentVersion;
            // Compare against the stable target, or against the latest version
            // (including prereleases) if the user opted in via Target=latest or --pre.
            var upgradeToVersion = settings.Target == Target.Latest || settings.Pre == true
                ? (p.LatestVersion ?? GetStableTargetVersion(p))
                : GetStableTargetVersion(p);

            if (upgradeToVersion is null || upgradeToVersion <= currentVersion)
            {
                p.UpgradeType = EUpgradeType.NoUpgrade;
                continue;
            }

            if (upgradeToVersion.Major != currentVersion.Major)
            {
                p.UpgradeType = EUpgradeType.Major;
                continue;
            }

            if (upgradeToVersion.Minor != currentVersion.Minor)
            {
                p.UpgradeType = EUpgradeType.Minor;
                continue;
            }

            if (upgradeToVersion.Patch != currentVersion.Patch)
            {
                p.UpgradeType = EUpgradeType.Patch;
                continue;
            }

            // At this point major/minor/patch are equal, but the version is still greater.
            // This covers prerelease bumps like 1.0.0-preview.1 -> 1.0.0-preview.2.
            p.UpgradeType = EUpgradeType.Patch;
        }

        return packages;
    }

    public static List<Package> PreparePackagesForOutput(List<Package> packages)
    {
        return packages.Where(p => p.UpgradeType != EUpgradeType.NoUpgrade).ToList();
    }

    public static List<Package> PreparePackagesForUpgrade(List<Package> packages, string upgradeTo)
    {
        return packages.Select(p =>
        {
            // Depending on the desired target version set the new version
            p.NewVersion = upgradeTo switch
            {
                Target.Stable => GetStableTargetVersion(p),
                Target.Latest => p.LatestVersion ?? p.LatestStableVersion ?? p.LatestPrereleaseVersion,
                _ => throw new ArgumentException(nameof(upgradeTo))
            };

            p.UpgradeTo = upgradeTo;

            return p;
        })
            // Only keep real upgrades. A lower version can be the latest one,
            // e.g. if the current version was unlisted, and must not cause a downgrade.
            .Where(p => p.NewVersion is not null && p.NewVersion > p.CurrentVersion).ToList();
    }
}

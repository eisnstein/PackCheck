using PackCheck.Commands.Settings;
using PackCheck.Data;

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
            packages = packages.Where(p => settings.Filter.Contains(p.PackageName)).ToList();
        }

        if (settings is { Exclude: { Length: > 0 } })
        {
            packages = packages.Where(p => !settings.Exclude.Contains(p.PackageName)).ToList();
        }

        return packages;
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
            // Determine which "lane" to compare against.
            // - If currently on a stable version, default to stable updates only.
            //   If user opted into latest/pre (Target=latest or --pre), include prerelease updates.
            // - If currently on a prerelease, compare against the latest prerelease.
            var upgradeToVersion = currentVersion.IsPrerelease
                ? (p.LatestPrereleaseVersion ?? p.LatestVersion ?? p.LatestStableVersion)
                : (settings.Target == Target.Latest || settings.Pre == true
                    ? (p.LatestVersion ?? p.LatestStableVersion)
                    : p.LatestStableVersion);

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
                // If the current version is prerelease, "stable" upgrades keep you on the prerelease lane.
                Target.Stable => p.CurrentVersion.IsPrerelease
                    ? (p.LatestPrereleaseVersion ?? p.LatestStableVersion)
                    : p.LatestStableVersion,
                Target.Latest => p.LatestVersion ?? p.LatestStableVersion ?? p.LatestPrereleaseVersion,
                _ => throw new ArgumentException(nameof(upgradeTo))
            };

            p.UpgradeTo = upgradeTo;

            return p;
        })
            // Filter out packages where nothing changes between current and new version
            .Where(p => p.NewVersion is not null && p.CurrentVersion != p.NewVersion).ToList();
    }
}

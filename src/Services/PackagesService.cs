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
            var upgradeToVersion = settings.Target switch
            {
                // If target is stable, but pre-releases are allowed, use latest version (which may be a pre-release); otherwise use latest stable version
                Target.Stable => settings.Pre == true ? p.LatestVersion : p.LatestStableVersion,
                Target.Latest => p.LatestVersion ?? p.LatestStableVersion,
                _ => throw new ArgumentException(nameof(settings.Target))
            };

            if (upgradeToVersion is null)
            {
                p.UpgradeType = EUpgradeType.NoUpgrade;
                continue;
            }

            if (currentVersion.Major != upgradeToVersion.Major)
            {
                p.UpgradeType = EUpgradeType.Major;
                continue;
            }

            if (currentVersion.Minor != upgradeToVersion.Minor)
            {
                p.UpgradeType = EUpgradeType.Minor;
                continue;
            }

            if (currentVersion.Patch != upgradeToVersion.Patch)
            {
                p.UpgradeType = EUpgradeType.Patch;
                continue;
            }

            p.UpgradeType = EUpgradeType.NoUpgrade;
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
                Target.Stable => p.LatestStableVersion,
                Target.Latest => p.LatestVersion ?? p.LatestStableVersion,
                _ => throw new ArgumentException(nameof(upgradeTo))
            };

            p.UpgradeTo = upgradeTo;

            return p;
        })
            // Filter out packages where nothing changes between current and new version
            .Where(p => p.CurrentVersion != p.NewVersion).ToList();
    }
}

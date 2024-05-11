using System;
using System.Collections.Generic;
using System.Linq;
using PackCheck.Data;

namespace PackCheck.Services;

public static class PackagesService
{
    public static List<Package> ApplyConfig(List<Package> packages, Config? config = null)
    {
        if (config is null)
        {
            return packages;
        }

        if (config is { Filter: { Count: > 0 } })
        {
            packages.Where(p => config.Filter.Contains(p.PackageName));
        }

        return packages;
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

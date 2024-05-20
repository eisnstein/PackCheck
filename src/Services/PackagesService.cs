using System;
using System.Collections.Generic;
using System.Linq;
using PackCheck.Commands.Settings;
using PackCheck.Data;

namespace PackCheck.Services;

public static class PackagesService
{
    public static List<Package> ApplySettings(List<Package> packages, CheckSettings? settings = null)
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

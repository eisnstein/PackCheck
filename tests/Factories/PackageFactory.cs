using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Tests.Factories;

public static class PackageFactory
{
    public static Package Create(string packageName, string currentVersion, string? latestStableVersion = null, string? latestVersion = null)
    {
        return new Package(packageName, NuGetVersion.Parse(currentVersion))
        {
            LatestStableVersion = latestStableVersion is not null ? NuGetVersion.Parse(latestStableVersion) : null,
            LatestVersion = latestVersion is not null ? NuGetVersion.Parse(latestVersion) : null
        };
    }
}

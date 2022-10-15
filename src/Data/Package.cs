using NuGet.Versioning;

namespace PackCheck.Data;

public class Package
{
    public string PackageName { get; }
    public string UpgradeTo { get; set; } = Version.Stable;
    public NuGetVersion CurrentVersion { get; }
    public NuGetVersion? NewVersion { get; set; }
    public NuGetVersion? LatestStableVersion { get; set; }
    public NuGetVersion? LatestVersion { get; set; }

    public Package(string name, NuGetVersion version)
    {
        PackageName = name;
        CurrentVersion = version;
    }
}

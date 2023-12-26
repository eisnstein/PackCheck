using NuGet.Versioning;

namespace PackCheck.Data;

public class Package(string name, NuGetVersion version)
{
    public string PackageName { get; } = name;
    public string UpgradeTo { get; set; } = Target.Stable;
    public NuGetVersion CurrentVersion { get; } = version;
    public NuGetVersion? NewVersion { get; set; }
    public NuGetVersion? LatestStableVersion { get; set; }
    public NuGetVersion? LatestVersion { get; set; }
}

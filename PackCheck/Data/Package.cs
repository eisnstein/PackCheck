using NuGet.Versioning;

namespace PackCheck.Data
{
    public class Package
    {
        public string PackageName { get; private init; }
        public NuGetVersion CurrentVersion { get; private init; }
        public NuGetVersion? LatestStableVersion { get; set; }
        public NuGetVersion? LatestVersion { get; set; }

        public Package(string name, NuGetVersion version)
        {
            PackageName = name;
            CurrentVersion = version;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Services
{
    public class NuGetVersionService
    {
        public NuGetVersion GetLatestStableVersion(Package package, IEnumerable<NuGetVersion> versions)
        {
            var majorVersions = versions
                .Where(v => v.Major == package.CurrentVersion.Major)
                .Where(v => v.IsPrerelease == false);

            return majorVersions.Last();
        }

        public NuGetVersion GetLatestVersion(Package package, IEnumerable<NuGetVersion> versions)
        {
            return versions.Last();
        }
    }
}

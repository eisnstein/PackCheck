using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Services
{
    public class NuGetVersionService
    {
        public NuGetVersion? GetLatestStableVersion(IEnumerable<NuGetVersion> versions)
        {
            // ReSharper disable once ReplaceWithSingleCallToLastOrDefault
            return versions
                .Where(v => v.IsPrerelease == false)
                .LastOrDefault();
        }

        public NuGetVersion GetLatestVersion(IEnumerable<NuGetVersion> versions)
        {
            return versions.Last();
        }
    }
}

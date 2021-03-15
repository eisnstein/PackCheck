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
            // ReSharper disable once ReplaceWithSingleCallToLast
            return versions
                .Where(v => v.IsPrerelease == false)
                .Last();
        }

        public NuGetVersion GetLatestVersion(Package package, IEnumerable<NuGetVersion> versions)
        {
            return versions.Last();
        }
    }
}

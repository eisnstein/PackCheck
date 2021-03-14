using System;
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
            return versions.Last(v => v.IsPrerelease == false);
        }

        public NuGetVersion GetLatestVersion(Package package, IEnumerable<NuGetVersion> versions)
        {
            return versions.Last();
        }
    }
}

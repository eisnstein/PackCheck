using System.Text;
using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Services
{
    public class PackageVersionHighlighterService
    {
        public string HighlightLatestStableVersion(Package p)
        {

            if (p.LatestStableVersion is null)
            {
                return "-";
            }

            return p.CurrentVersion == p.LatestStableVersion
                ? p.CurrentVersion.ToString()
                : HighlightVersion(p.CurrentVersion, p.LatestStableVersion);
        }

        public string HighlightLatestVersion(Package p)
        {
            if (p.LatestVersion is null)
            {
                return "-";
            }

            return p.CurrentVersion == p.LatestVersion
                ? p.CurrentVersion.ToString()
                : HighlightVersion(p.CurrentVersion, p.LatestVersion);
        }

        private string HighlightVersion(NuGetVersion current, NuGetVersion other)
        {
            var highlightedVersion = new StringBuilder();
            if (current.Major == other.Major)
            {
                highlightedVersion
                    .Append(other.Major)
                    .Append('.');

                if (current.Minor == other.Minor)
                {
                    highlightedVersion
                        .Append(other.Minor)
                        .Append('.')
                        .Append("[green]")
                        .Append(other.Patch)
                        .Append("[/]");
                }
            }
            else
            {
                return $"[red]{other.ToString()}[/]";
            }

            return highlightedVersion.ToString();
        }
    }
}

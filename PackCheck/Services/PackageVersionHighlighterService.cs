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

        private string HighlightVersion(NuGetVersion current, NuGetVersion newer)
        {
            // If the newer major version is greater than the current major
            // version we have a breaking change and can return all red.
            if (newer.Major > current.Major)
            {
                return $"[red]{newer.ToString()}[/]";
            }

            var highlightedVersion = new StringBuilder();
            highlightedVersion
                .Append(newer.Major)
                .Append('.');

            if (newer.Minor > current.Minor)
            {
                highlightedVersion
                    .Append("[yellow]")
                    .Append(newer.Minor)
                    .Append('.')
                    .Append(newer.Patch);

                if (newer.IsPrerelease)
                {
                    highlightedVersion
                        .Append('-')
                        .Append(newer.Release);
                }

                highlightedVersion
                    .Append("[/]");

                return highlightedVersion.ToString();
            }

            if (newer.Patch > current.Patch)
            {
                highlightedVersion
                    .Append(newer.Minor)
                    .Append('.')
                    .Append("[green]")
                    .Append(newer.Patch);

                if (newer.IsPrerelease)
                {
                    highlightedVersion
                        .Append('-')
                        .Append(newer.Release);
                }

                highlightedVersion
                    .Append("[/]");

                return highlightedVersion.ToString();
            }

            highlightedVersion
                .Append(newer.Minor)
                .Append('.')
                .Append(newer.Patch);

            if (newer.IsPrerelease)
            {
                highlightedVersion
                    .Append("[green]")
                    .Append('-')
                    .Append(newer.Release)
                    .Append("[/]");
            }
            else if (current.IsPrerelease)
            {
                highlightedVersion
                    .Insert(0, "[green]")
                    .Append("[/]");
            }

            return highlightedVersion.ToString();
        }
    }
}

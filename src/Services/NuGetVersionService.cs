using NuGet.Versioning;

namespace PackCheck.Services;

public static class NuGetVersionService
{
    public static NuGetVersion? GetLatestStableVersion(IReadOnlyList<NuGetVersion> versions)
    {
        // ReSharper disable once ReplaceWithSingleCallToLastOrDefault
        return versions
            .Where(v => v.IsPrerelease == false)
            .LastOrDefault();
    }

    public static NuGetVersion GetLatestVersion(IReadOnlyList<NuGetVersion> versions)
    {
        return versions.Last();
    }
}

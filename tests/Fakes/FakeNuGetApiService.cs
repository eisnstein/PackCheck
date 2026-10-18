using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Fakes;

/// <summary>
/// Returns fixed versions instead of querying nuget.org, so command tests and
/// their snapshots do not change whenever a package is released.
/// Covers the packages in TestData/test.csproj.
/// </summary>
public class FakeNuGetApiService : INuGetApiService
{
    private static readonly Dictionary<string, string[]> Versions = new()
    {
        // Major upgrade
        ["NuGet.Protocol"] = ["6.2.1", "6.2.2", "7.6.0"],
        // Minor upgrade, with a newer prerelease
        ["NuGet.Versioning"] = ["6.2.1", "6.3.0", "6.4.0-preview.1"],
        // Already up to date
        ["Spectre.Cli.Extensions.DependencyInjection"] = ["0.3.0", "0.4.0"],
        // Patch upgrade, with a newer prerelease
        ["Spectre.Console"] = ["0.44.0", "0.44.1", "0.45.0-alpha.0.1"],
    };

    public Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken)
    {
        IEnumerable<NuGetVersion> versions = Versions.TryGetValue(packageName, out var found)
            ? found.Select(NuGetVersion.Parse).Order().ToList()
            : [];

        return Task.FromResult(versions);
    }
}

using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Fakes;

/// <summary>
/// Behaves like the NuGet feed being unreachable, e.g. when offline.
/// </summary>
public class UnreachableNuGetApiService : INuGetApiService
{
    public Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken)
    {
        throw new HttpRequestException("No such host is known. (api.nuget.org:443)");
    }
}

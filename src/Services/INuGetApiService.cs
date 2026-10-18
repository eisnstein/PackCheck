using NuGet.Versioning;

namespace PackCheck.Services;

public interface INuGetApiService
{
    Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken);
}

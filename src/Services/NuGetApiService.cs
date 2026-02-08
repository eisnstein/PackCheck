using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PackCheck.Services;

public class NuGetApiService
{
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly SourceCacheContext _cache = new();
    private readonly FindPackageByIdResource _findPackageByIdResource;
    private readonly PackageMetadataResource _packageMetadataResource;

    public NuGetApiService()
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        _findPackageByIdResource = repository.GetResource<FindPackageByIdResource>(CancellationToken.None);
        _packageMetadataResource = repository.GetResource<PackageMetadataResource>(CancellationToken.None);
    }

    public async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return Array.Empty<NuGetVersion>();
        }

        // NOTE: FindPackageByIdResource.GetAllVersionsAsync may return unlisted versions.
        // PackageMetadataResource supports excluding unlisted packages via includeUnlisted: false.
        var metadata = await _packageMetadataResource.GetMetadataAsync(
            packageName,
            includePrerelease: true,
            includeUnlisted: false,
            _cache,
            _logger,
            cancellationToken);

        if (metadata is not null)
        {
            return metadata
                .Select(m => m.Identity?.Version)
                .Where(v => v is not null)
                .Cast<NuGetVersion>()
                .Distinct()
                .Order()
                .ToList();
        }

        // Fallback: should be rare, but keep behavior for feeds that don't support metadata resource well.
        return await _findPackageByIdResource.GetAllVersionsAsync(packageName, _cache, _logger, cancellationToken);
    }
}

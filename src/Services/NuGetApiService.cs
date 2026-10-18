using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PackCheck.Services;

public class NuGetApiService : INuGetApiService
{
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly SourceCacheContext _cache = new();
    private readonly Lazy<Task<FindPackageByIdResource>> _findPackageByIdResource;
    private readonly Lazy<Task<PackageMetadataResource>> _packageMetadataResource;

    public NuGetApiService()
    {
        // Getting a resource downloads the feed's service index, so it is done lazily
        // on first use. Otherwise creating this service (and with it every command)
        // would already fail when the feed is unreachable.
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        _findPackageByIdResource = new(() => GetResourceAsync<FindPackageByIdResource>(repository));
        _packageMetadataResource = new(() => GetResourceAsync<PackageMetadataResource>(repository));
    }

    private static async Task<T> GetResourceAsync<T>(SourceRepository repository) where T : class, INuGetResource
    {
        return await repository.GetResourceAsync<T>(CancellationToken.None)
            ?? throw new InvalidOperationException($"The NuGet feed does not provide a {typeof(T).Name}.");
    }

    public async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return Array.Empty<NuGetVersion>();
        }

        // NOTE: FindPackageByIdResource.GetAllVersionsAsync may return unlisted versions.
        // PackageMetadataResource supports excluding unlisted packages via includeUnlisted: false.
        var packageMetadataResource = await _packageMetadataResource.Value;
        var metadata = await packageMetadataResource.GetMetadataAsync(
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
        var findPackageByIdResource = await _findPackageByIdResource.Value;
        return await findPackageByIdResource.GetAllVersionsAsync(packageName, _cache, _logger, cancellationToken);
    }
}

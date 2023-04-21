using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PackCheck.Services;

public class NuGetApiService
{
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly SourceCacheContext _cache = new();
    private readonly FindPackageByIdResource _resource;

    public NuGetApiService()
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        _resource = repository.GetResource<FindPackageByIdResource>(_cancellationToken);
    }

    public Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName)
    {
        return _resource.GetAllVersionsAsync(packageName, _cache, _logger, _cancellationToken);
    }
}

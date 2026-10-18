using NuGet.Versioning;
using PackCheck.Services;

namespace PackCheck.Tests.Services;

public class PackCheckServiceTests
{
    [Test]
    public async Task CheckForNewPackCheckVersion_DoesNotThrow_When_FeedIsUnreachable()
    {
        var nuGetApiService = new UnreachableNuGetApiService();

        await Assert.That(() => PackCheckService.CheckForNewPackCheckVersion(nuGetApiService)).ThrowsNothing();
    }

    private class UnreachableNuGetApiService : INuGetApiService
    {
        public Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("No such host is known. (api.nuget.org:443)");
        }
    }
}

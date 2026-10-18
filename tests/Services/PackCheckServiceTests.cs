using PackCheck.Services;
using PackCheck.Tests.Fakes;

namespace PackCheck.Tests.Services;

public class PackCheckServiceTests
{
    [Test]
    public async Task CheckForNewPackCheckVersion_DoesNotThrow_When_FeedIsUnreachable()
    {
        var nuGetApiService = new UnreachableNuGetApiService();

        await Assert.That(() => PackCheckService.CheckForNewPackCheckVersion(nuGetApiService)).ThrowsNothing();
    }
}

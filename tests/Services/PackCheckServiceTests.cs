using PackCheck.Services;
using PackCheck.Tests.Fakes;
using Spectre.Console.Testing;

namespace PackCheck.Tests.Services;

public class PackCheckServiceTests
{
    [Test]
    public async Task CheckForNewPackCheckVersion_DoesNotThrow_When_FeedIsUnreachable()
    {
        var nuGetApiService = new UnreachableNuGetApiService();

        await Assert.That(() => PackCheckService.CheckForNewPackCheckVersion(nuGetApiService, new TestConsole())).ThrowsNothing();
    }
}

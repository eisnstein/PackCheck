using PackCheck.Exceptions;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests
{
    public class CsProjFileServiceTests
    {
        [Fact]
        public void ThrowsWhenPathToCsProjFileIsProvidedButFileDoesNotExist()
        {
            var service = new CsProjFileService();
            var pathToFile = "does.not.exist.csproj";

            Assert.Throws<CsProjFileNotFoundException>(() => service.GetPathToCsProjFile(pathToFile));
        }
    }
}

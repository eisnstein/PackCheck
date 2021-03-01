using System;
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
            var pathToFile = "does-not-exist.csproj";

            Action actual = () => service.GetPathToCsProjFile(pathToFile);

            Assert.Throws<CsProjFileNotFoundException>(actual);
        }
    }
}

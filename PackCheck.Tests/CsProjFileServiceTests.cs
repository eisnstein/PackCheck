using System;
using PackCheck.Exceptions;
using PackCheck.Services;
using Xunit;

namespace PackCheck.Tests
{
    public class CsProjFileServiceTests
    {
        private readonly CsProjFileService _service;

        public CsProjFileServiceTests()
        {
            _service = new();
        }

        [Fact]
        public void ThrowsWhenPathToCsProjFileIsProvidedButFileDoesNotExist()
        {
            var pathToFile = "does-not-exist.csproj";

            Action actual = () => _service.GetPathToCsProjFile(pathToFile);

            Assert.Throws<CsProjFileNotFoundException>(actual);
        }

        [Fact]
        public void CsProjFileExistsAtGivenPath()
        {
            var relativePath = "test.csproj";

            var fullPath = _service.GetPathToCsProjFile(relativePath);

            Assert.EndsWith(relativePath, fullPath);
        }

        [Fact]
        public void CsProjFileExistsWithoutGivenPath()
        {
            var relativePath = "test.csproj";

            var fullPath = _service.GetPathToCsProjFile();

            Assert.EndsWith(relativePath, fullPath);
        }
    }
}

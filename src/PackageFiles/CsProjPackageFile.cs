using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.PackageFiles;

public sealed record CsProjPackageFile(string Path) : IPackageFile
{
    public Task<List<Package>> ReadPackagesAsync()
    {
        return CsProjFileService.GetPackagesDataFromCsProjFileAsync(Path);
    }

    public Task WriteVersionsAsync(IReadOnlyList<Package> packages, bool dryRun)
    {
        return CsProjFileService.UpgradePackageVersionsAsync(Path, packages, dryRun);
    }
}

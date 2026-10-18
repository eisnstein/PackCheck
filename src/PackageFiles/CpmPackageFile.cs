using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.PackageFiles;

/// <summary>
/// A Directory.Packages.props file (Central Package Management).
/// </summary>
public sealed record CpmPackageFile(string Path) : IPackageFile
{
    public Task<List<Package>> ReadPackagesAsync()
    {
        return CentralPackageMgmtService.GetPackagesDataFromCpmFileAsync(Path);
    }

    public Task WriteVersionsAsync(IReadOnlyList<Package> packages, bool dryRun)
    {
        return CentralPackageMgmtService.UpgradePackageVersionsAsync(Path, packages, dryRun);
    }
}

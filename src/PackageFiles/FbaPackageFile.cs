using PackCheck.Data;
using PackCheck.Services;

namespace PackCheck.PackageFiles;

/// <summary>
/// A file-based app, a single .cs file with #:package directives.
/// </summary>
public sealed record FbaPackageFile(string Path) : IPackageFile
{
    public Task<List<Package>> ReadPackagesAsync()
    {
        return Task.FromResult(FileBasedAppService.GetPackagesDataFromFbaFile(Path));
    }

    public Task WriteVersionsAsync(IReadOnlyList<Package> packages, bool dryRun)
    {
        FileBasedAppService.UpgradePackageVersions(Path, packages.ToList(), dryRun);
        return Task.CompletedTask;
    }
}

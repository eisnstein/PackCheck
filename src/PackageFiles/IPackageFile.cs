using PackCheck.Data;

namespace PackCheck.PackageFiles;

/// <summary>
/// A file that lists package versions, which check reads and upgrade changes:
/// a .csproj, a Directory.Packages.props or a file-based app.
/// </summary>
public interface IPackageFile
{
    /// <summary>Full path to the file.</summary>
    string Path { get; }

    Task<List<Package>> ReadPackagesAsync();

    /// <summary>
    /// Writes the NewVersion of the given packages to the file,
    /// or only prints the result if dryRun is set.
    /// </summary>
    Task WriteVersionsAsync(IReadOnlyList<Package> packages, bool dryRun);
}

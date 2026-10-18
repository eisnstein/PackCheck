namespace PackCheck.Data;

/// <summary>
/// The result for one file in the --output json/markdown document.
/// </summary>
/// <param name="Path">The path relative to the current directory.</param>
/// <param name="Packages">
/// check: the outdated packages. upgrade: the upgraded packages, with NewVersion set.
/// Empty if there is nothing to do.
/// </param>
public sealed record FileReport(string Path, IReadOnlyList<Package> Packages);

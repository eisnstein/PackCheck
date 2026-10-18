using PackCheck.Commands.Settings;
using PackCheck.PackageFiles;

namespace PackCheck.Data;

/// <summary>
/// The initial state of a check or upgrade run, built once before any file is processed.
/// </summary>
/// <param name="Settings">The CLI settings, merged with the values from the config file.</param>
/// <param name="PathToConfigFile">The config file that was read, if any.</param>
/// <param name="Files">The files to check or upgrade, all of which exist.</param>
/// <param name="IsSolution">Whether the files are the projects of a .sln or .slnx file.</param>
public sealed record PackCheckContext<TSettings>(
    TSettings Settings,
    string? PathToConfigFile,
    IReadOnlyList<IPackageFile> Files,
    bool IsSolution) where TSettings : CommonSettings;

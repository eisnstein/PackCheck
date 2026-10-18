using PackCheck.Commands.Settings;
using PackCheck.PackageFiles;

namespace PackCheck.Services;

/// <summary>
/// Builds the parts of a <see cref="Data.PackCheckContext{TSettings}"/>.
/// </summary>
public static class PackCheckContextBuilder
{
    /// <summary>
    /// Reads the config file (if any) and merges it into the CLI settings.
    /// Throws a JsonException if the config file is invalid.
    /// </summary>
    public static (TSettings Settings, string? PathToConfigFile) LoadSettings<TSettings>(TSettings settings)
        where TSettings : CommonSettings
    {
        var (pathToConfigFile, config) = PackCheckConfigService.GetConfig();

        return (SettingsService.CombineSettingsWithConfig(settings, config), pathToConfigFile);
    }

    /// <summary>
    /// Decides which files to check or upgrade: a path given via the CLI or the
    /// config file wins, otherwise the current directory is searched. Throws the
    /// file exceptions (e.g. CsProjFileException) if a file does not exist or is ambiguous.
    /// </summary>
    public static (IReadOnlyList<IPackageFile> Files, bool IsSolution) ResolveFiles(CommonSettings settings)
    {
        // Paths given via the CLI or the config file
        if (!string.IsNullOrEmpty(settings.PathToCsProjFile))
        {
            return ([new CsProjPackageFile(CsProjFileService.GetPathToCsProjFile(settings.PathToCsProjFile))], false);
        }

        if (!string.IsNullOrEmpty(settings.PathToSlnFile))
        {
            return (ProjectsOf(SolutionFileService.GetCsProjFiles(SolutionFileService.GetPathToSolutionFile(settings.PathToSlnFile))), true);
        }

        if (!string.IsNullOrEmpty(settings.PathToSlnxFile))
        {
            return (ProjectsOf(SolutionXFileService.GetCsProjFiles(SolutionXFileService.GetPathToSolutionXFile(settings.PathToSlnxFile))), true);
        }

        if (!string.IsNullOrEmpty(settings.PathToCpmFile))
        {
            return ([new CpmPackageFile(CentralPackageMgmtService.GetPathToCpmFile(settings.PathToCpmFile))], false);
        }

        if (!string.IsNullOrEmpty(settings.PathToFbaFile))
        {
            return ([new FbaPackageFile(FileBasedAppService.GetPathToFileBasedAppFile(settings.PathToFbaFile))], false);
        }

        // Nothing given, so search the current directory
        if (CentralPackageMgmtService.HasCentralPackageMgmt())
        {
            return ([new CpmPackageFile(CentralPackageMgmtService.GetPathToCpmFile())], false);
        }

        if (SolutionFileService.HasSolution())
        {
            return (ProjectsOf(SolutionFileService.GetCsProjFiles(SolutionFileService.GetPathToSolutionFile())), true);
        }

        if (SolutionXFileService.HasSolutionX())
        {
            return (ProjectsOf(SolutionXFileService.GetCsProjFiles(SolutionXFileService.GetPathToSolutionXFile())), true);
        }

        return ([new CsProjPackageFile(CsProjFileService.GetPathToCsProjFile())], false);
    }

    // Checks that every project of the solution exists before anything is processed
    private static List<IPackageFile> ProjectsOf(List<string> csProjFiles)
    {
        return csProjFiles
            .Select(IPackageFile (path) => new CsProjPackageFile(CsProjFileService.GetPathToCsProjFile(path)))
            .ToList();
    }
}

using System.Xml;
using PackCheck.Exceptions;

namespace PackCheck.Services;

public static class SolutionXFileService
{
    public static bool HasSolutionX()
    {
        var cwd = Directory.GetCurrentDirectory();
        return Directory.EnumerateFiles(cwd, "*.slnx").Any();
    }

    public static string GetPathToSolutionXFile(string? pathToSolutionXFile = null)
    {
        var cwd = Directory.GetCurrentDirectory();

        // A path to the .sln file is given
        if (!string.IsNullOrEmpty(pathToSolutionXFile))
        {
            // If pathToSlnFile is already a full path, it will not
            // be combined with the current directory.
            var fullPath = Path.Combine(cwd, pathToSolutionXFile);
            if (!File.Exists(fullPath))
            {
                throw new SolutionXFileException(
                    $"File [white]{pathToSolutionXFile}[/] does not exist in the current directory [white]{cwd}[/]"
                );
            }

            return fullPath;
        }

        // No path was provided, we try to find a .slnx file
        var files = Directory.GetFiles(cwd, "*.slnx");

        return files.Length switch
        {
            0 => throw new SolutionXFileException($"Could not find a .slnx file in the current directory [white]{cwd}[/]"),
            > 1 => throw new SolutionXFileException($"Found more than 1 .slnx file. Please provide the .slnx file to use via the --slnxFile argument. [white]{cwd}[/]"),
            _ => Path.Combine(cwd, files[0])
        };
    }

    /// <summary>
    /// Returns the full paths of all .csproj files in the solution. The paths in
    /// a .slnx file are relative to the solution file, not the current directory.
    /// </summary>
    public static List<string> GetCsProjFiles(string pathToSolutionXFile)
    {
        var solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(pathToSolutionXFile))!;

        return ParseProjectDefinitions(pathToSolutionXFile)
            .Select(projectPath => Path.GetFullPath(Path.Combine(solutionDirectory, projectPath)))
            .ToList();
    }

    public static List<string> ParseProjectDefinitions(string pathToSolutionXFile)
    {
        List<string> projectPaths = new();
        var settings = new XmlReaderSettings { Async = false };
        using var reader = XmlReader.Create(pathToSolutionXFile, settings);

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "Project")
            {
                continue;
            }

            if (!reader.HasAttributes)
            {
                continue;
            }

            var path = "";

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "Path")
                {
                    path = reader.Value;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            // Visual Studio writes the project paths with backslashes,
            // which do not work as separators on unix like systems
            projectPaths.Add(path.Replace('\\', Path.DirectorySeparatorChar));
        }

        return projectPaths;
    }
}

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
                throw new SolutionFileException(
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

    public static List<string> ParseProjectDefinitions(string pathToSolutionXFile)
    {
        List<string> projectPaths = new();
        var settings = new XmlReaderSettings { Async = false };
        var reader = XmlReader.Create(pathToSolutionXFile, settings);

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

            projectPaths.Add(path);
        }

        reader.Close();

        return projectPaths;
    }
}

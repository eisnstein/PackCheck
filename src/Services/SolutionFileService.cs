using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PackCheck.Exceptions;

namespace PackCheck.Services
{
    public class SolutionFileService
    {
        public bool HasSolution()
        {
            var cwd = Directory.GetCurrentDirectory();
            return Directory.EnumerateFiles(cwd, "*.sln").Any();
        }

        public string GetPathToSolutionFile(string? pathToSolutionFile = null)
        {
            var cwd = Directory.GetCurrentDirectory();

            // The user provided a path to the .csproj file
            if (!string.IsNullOrEmpty(pathToSolutionFile))
            {
                // If pathToSlnFile is already a full path, it will not
                // be combined with the current directory.
                var fullPath = Path.Combine(cwd, pathToSolutionFile);
                if (!File.Exists(fullPath))
                {
                    throw new SolutionFileException(
                        $"File [white]{pathToSolutionFile}[/] does not exist in the current directory [white]{cwd}[/]"
                    );
                }

                return fullPath;
            }

            // No path was provided, we try to find a .sln file
            var files = Directory.GetFiles(cwd, "*.sln");

            return files.Length switch
            {
                0 => throw new SolutionFileException($"Could not find a .sln file in the current directory [white]{cwd}[/]"),
                > 1 => throw new SolutionFileException($"Found more than 1 .sln file. Please provide the .sln file to use via the --slnFile argument. [white]{cwd}[/]"),
                _ => Path.Combine(cwd, files[0])
            };
        }

        public IEnumerable<string> GetProjectDefinitions(string pathToSolutionFile)
        {
            return File.ReadAllLines(pathToSolutionFile)
                .Where(line => line.StartsWith("Project("));
        }

        public List<string> ParseProjectDefinitions(IEnumerable<string> projectDefinitions)
        {
            // A project definition looks like that: Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "src", "src\src.csproj", "{74E0B439-9BC7-4CBD-A4C0-5E0248A70664}"
            var pattern = @"^Project\(""\{[A-Za-z0-9\-]+\}""\) = "".+"", ""(?<path>.+\.csproj)"", ""\{[A-Za-z0-9\-]+\}""$";
            var rgx = new Regex(pattern, RegexOptions.Compiled);

            return projectDefinitions
                .Select(definition =>
                {
                    var match = rgx.Match(definition);
                    if (match.Success)
                    {
                        return match.Groups["path"].Value;
                    }

                    return string.Empty;
                })
                .Where(projectPath => !string.IsNullOrEmpty(projectPath))
                .ToList();
        }
    }
}

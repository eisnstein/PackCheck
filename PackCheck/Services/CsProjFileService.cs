using System;
using System.IO;
using PackCheck.Exceptions;

namespace PackCheck.Services
{
    public class CsProjFileService
    {
        public string GetPathToCsProjFile(string? pathToCsProjFile)
        {
            var cwd = Directory.GetCurrentDirectory();

            // The user provided a path to the .csproj file
            if (!string.IsNullOrEmpty(pathToCsProjFile))
            {
                var fullPath = Path.Combine(cwd, pathToCsProjFile);
                if (!File.Exists(fullPath))
                {
                    throw new CsProjFileNotFoundException(
                        $"File [white]{pathToCsProjFile}[/] does not exist in the current directory [white]{cwd}[/]"
                    );
                }

                return pathToCsProjFile;
            }

            // No path was provided, we try to find a .csproj file
            var files = Directory.GetFiles(cwd, "*.csproj");

            return files.Length switch
            {
                0 => throw new CsProjFileNotFoundException(
                    $"Could not find a .csproj file in the current directory [white]{cwd}[/]"
                ),
                _ => files[0]
            };
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PackCheck.Services;

public static class PackCheckConfigService
{
    private static readonly string PackCheckConfigFileName = ".packcheckrc";

    public static bool HasPackCheckConfigFile()
    {
        var cwd = Directory.GetCurrentDirectory();
        var configFile = Path.Combine(cwd, PackCheckConfigFileName);
        if (File.Exists(configFile))
        {
            return true;
        }

        configFile = Path.Combine(cwd, PackCheckConfigFileName + ".json");
        if (File.Exists(configFile))
        {
            return true;
        }

        return false;
    }

    public static Config? GetConfig()
    {
        var cwd = Directory.GetCurrentDirectory();
        var configFile = Path.Combine(cwd, PackCheckConfigFileName);
        if (File.Exists(configFile))
        {
            return ParseConfigFile(configFile);
        }

        configFile = Path.Combine(cwd, PackCheckConfigFileName + ".json");
        if (File.Exists(configFile))
        {
            return ParseConfigFile(configFile);
        }

        return null;
    }

    private static Config? ParseConfigFile(string pathToConfigFile)
    {
        string configJsonStr = File.ReadAllText(pathToConfigFile);
        return JsonSerializer.Deserialize<Config>(configJsonStr);
    }
}

public record Config
{
    public List<string>? Exclude { get; init; }
    public List<string>? Filter { get; init; }
}

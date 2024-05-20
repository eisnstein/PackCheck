using System.IO;
using System.Text.Json;
using PackCheck.Data;

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

    public static (string? configFile, Config? config) GetConfig()
    {
        var cwd = Directory.GetCurrentDirectory();
        var configFile = Path.Combine(cwd, PackCheckConfigFileName);
        if (File.Exists(configFile))
        {
            return (configFile, ParseConfigFile(configFile));
        }

        configFile = Path.Combine(cwd, PackCheckConfigFileName + ".json");
        if (File.Exists(configFile))
        {
            return (configFile, ParseConfigFile(configFile));
        }

        return (null, null);
    }

    private static Config? ParseConfigFile(string pathToConfigFile)
    {
        var options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        string configJsonStr = File.ReadAllText(pathToConfigFile);
        return JsonSerializer.Deserialize<Config>(configJsonStr, options);
    }
}

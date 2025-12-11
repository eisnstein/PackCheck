using PackCheck.Commands.Settings;
using PackCheck.Data;

namespace PackCheck.Services;

public static class SettingsService
{
    public static CheckSettings CombineSettingsWithConfig(CheckSettings settings, Config? config)
    {
        if (config is null)
        {
            return settings;
        }

        // Settings given via the CLI have precedence over values from the config file.
        settings.PathToCsProjFile ??= config.CsProjFile;
        settings.PathToSlnFile ??= config.SlnFile;
        settings.PathToSlnxFile ??= config.SlnxFile;
        settings.PathToCpmFile ??= config.CpmFile;
        settings.Filter ??= config.Filter?.ToArray();
        settings.Exclude ??= config.Exclude?.ToArray();
        settings.Format ??= config.Format;

        // If Pre is not set in CLI,
        // take the value from config which is false by default.
        if (settings.Pre is null)
        {
            settings.Pre = config.Pre;
        }

        return settings;
    }

    public static UpgradeSettings CombineSettingsWithConfig(UpgradeSettings settings, Config? config)
    {
        if (config is null)
        {
            return settings;
        }

        // Settings given via the CLI have precedence over values from the config file.
        settings.PathToCsProjFile ??= config.CsProjFile;
        settings.PathToSlnFile ??= config.SlnFile;
        settings.PathToSlnxFile ??= config.SlnxFile;
        settings.PathToCpmFile ??= config.CpmFile;
        settings.Filter ??= config.Filter?.ToArray();
        settings.Exclude ??= config.Exclude?.ToArray();

        return settings;
    }
}

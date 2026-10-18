using PackCheck.Commands.Settings;
using PackCheck.Data;

namespace PackCheck.Services;

public static class SettingsService
{
    public static TSettings CombineSettingsWithConfig<TSettings>(TSettings settings, Config? config)
        where TSettings : CommonSettings
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
        settings.PathToFbaFile ??= config.FbaFile;
        settings.Filter ??= config.Filter?.ToArray();
        settings.Exclude ??= config.Exclude?.ToArray();
        settings.Output ??= config.Output;

        // Options that only exist for the check command
        if (settings is CheckSettings checkSettings)
        {
            checkSettings.Format ??= config.Format;

            // If Pre is not set in CLI,
            // take the value from config which is false by default.
            checkSettings.Pre ??= config.Pre;
        }

        return settings;
    }
}

using SFDCT.Helper;
using SFDCT.Misc;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SFDCT.Configuration;

internal static class SFDCTConfig
{
    private static CTIniHandler ConfigHandler;
    private static readonly Dictionary<CTSettingKey, object> Settings = [];

    internal static void SaveFile()
    {
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundPanningEnabled), Get<bool>(CTSettingKey.SoundPanningEnabled));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundPanningStrength), Get<float>(CTSettingKey.SoundPanningStrength));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundPanningForceScreenSpace), Get<bool>(CTSettingKey.SoundPanningForceScreenSpace));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundPanningInworldThreshold), Get<int>(CTSettingKey.SoundPanningInworldThreshold));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundPanningInworldDistance), Get<int>(CTSettingKey.SoundPanningInworldDistance));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundAttenuationEnabled), Get<bool>(CTSettingKey.SoundAttenuationEnabled));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundAttenuationMin), Get<float>(CTSettingKey.SoundAttenuationMin));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundAttenuationForceScreenSpace), Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundAttenuationInworldThreshold), Get<int>(CTSettingKey.SoundAttenuationInworldThreshold));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SoundAttenuationInworldDistance), Get<int>(CTSettingKey.SoundAttenuationInworldDistance));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.LowHealthSaturationFactor), Get<float>(CTSettingKey.LowHealthSaturationFactor));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.LowHealthThreshold), Get<float>(CTSettingKey.LowHealthThreshold));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.LowHealthHurtLevel1Threshold), Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.LowHealthHurtLevel2Threshold), Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.HideFilmgrain), Get<bool>(CTSettingKey.HideFilmgrain));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.DisableClockTicking), Get<bool>(CTSettingKey.DisableClockTicking));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.Language), Get<string>(CTSettingKey.Language));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SpectatorsMaximum), Get<int>(CTSettingKey.SpectatorsMaximum));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.SpectatorsOnlyModerators), Get<bool>(CTSettingKey.SpectatorsOnlyModerators));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.VoteKickEnabled), Get<bool>(CTSettingKey.VoteKickEnabled));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.VoteKickFailCooldown), Get<int>(CTSettingKey.VoteKickFailCooldown));
        ConfigHandler.UpdateValue(GetSettingKey(CTSettingKey.VoteKickSuccessCooldown), Get<int>(CTSettingKey.VoteKickSuccessCooldown));

        ConfigHandler.SaveFile(Globals.Paths.ConfigurationIni);
    }

    internal static void LoadFile()
    {
        ConfigHandler = new();
        SetDefaultSettings();

        if (!File.Exists(Globals.Paths.ConfigurationIni))
        {
            // Create file and revert to defaults
            Logger.LogDebug("Setting CREATING");

            using (FileStream fileStream = File.Create(Globals.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }

            Thread.Sleep(100);

            ConfigHandler.AppendEmptyLines = true;
            ConfigHandler.AutoAddMissingValues = true;

            Logger.LogDebug("Setting UPDATING TO DEFAULTS");
            LoadDefault();

            SaveFile();

            ConfigHandler.AppendEmptyLines = false;
            ConfigHandler.AutoAddMissingValues = false;
        }
        else
        {
            // Load file and read settings
            Logger.LogDebug("Setting READING FILE");

            ConfigHandler.ReadFile(Globals.Paths.ConfigurationIni);

            Logger.LogDebug("Setting UPDATING");
            Set(CTSettingKey.SoundPanningEnabled, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.SoundPanningEnabled), true));
            Set(CTSettingKey.SoundPanningStrength, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.SoundPanningStrength), 0.71f, 0f, 1f));
            Set(CTSettingKey.SoundPanningForceScreenSpace, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.SoundPanningForceScreenSpace), false));
            Set(CTSettingKey.SoundPanningInworldThreshold, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundPanningInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundPanningInworldDistance, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundPanningInworldDistance), 400, 0, 1000));
            Set(CTSettingKey.SoundAttenuationEnabled, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.SoundAttenuationEnabled), true));
            Set(CTSettingKey.SoundAttenuationMin, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.SoundAttenuationMin), 0.6f, 0f, 1f));
            Set(CTSettingKey.SoundAttenuationForceScreenSpace, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.SoundAttenuationForceScreenSpace), false));
            Set(CTSettingKey.SoundAttenuationInworldThreshold, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundAttenuationInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundAttenuationInworldDistance, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundAttenuationInworldDistance), 500, 0, 1000));
            Set(CTSettingKey.LowHealthSaturationFactor, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthSaturationFactor), 0.71f, 0f, 1f));
            Set(CTSettingKey.LowHealthThreshold, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthThreshold), 0.25f, 0f, 1f));
            Set(CTSettingKey.LowHealthHurtLevel1Threshold, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthHurtLevel1Threshold), 0.25f, 0f, 1f));
            Set(CTSettingKey.LowHealthHurtLevel2Threshold, ConfigHandler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthHurtLevel2Threshold), 0.12f, 0f, 1f));
            Set(CTSettingKey.HideFilmgrain, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.HideFilmgrain), false));
            Set(CTSettingKey.DisableClockTicking, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.DisableClockTicking), true));
            Set(CTSettingKey.Language, ConfigHandler.ReadValueString(GetSettingKey(CTSettingKey.Language), "SFDCT_Default"));
            Set(CTSettingKey.SpectatorsMaximum, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SpectatorsMaximum), 4, 0, 4));
            Set(CTSettingKey.SpectatorsOnlyModerators, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.SpectatorsOnlyModerators), true));
            Set(CTSettingKey.VoteKickEnabled, ConfigHandler.ReadValueBool(GetSettingKey(CTSettingKey.VoteKickEnabled), false));
            Set(CTSettingKey.VoteKickFailCooldown, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.VoteKickFailCooldown), 150, 30, 300));
            Set(CTSettingKey.VoteKickSuccessCooldown, ConfigHandler.ReadValueIntCapped(GetSettingKey(CTSettingKey.VoteKickSuccessCooldown), 60, 30, 300));
        }
    }

    internal static void SetDefaultSettings()
    {
        Logger.LogDebug("Setting REVERING TO DEFAULTS");

        Set(CTSettingKey.SoundPanningEnabled, true);
        Set(CTSettingKey.SoundPanningStrength, 0.71f);
        Set(CTSettingKey.SoundPanningForceScreenSpace, false);
        Set(CTSettingKey.SoundPanningInworldThreshold, 60);
        Set(CTSettingKey.SoundPanningInworldDistance, 400);
        Set(CTSettingKey.SoundAttenuationEnabled, true);
        Set(CTSettingKey.SoundAttenuationMin, 0.6f);
        Set(CTSettingKey.SoundAttenuationForceScreenSpace, false);
        Set(CTSettingKey.SoundAttenuationInworldThreshold, 60);
        Set(CTSettingKey.SoundAttenuationInworldDistance, 500);
        Set(CTSettingKey.LowHealthSaturationFactor, 0.71f);
        Set(CTSettingKey.LowHealthThreshold, 0.25f);
        Set(CTSettingKey.LowHealthHurtLevel1Threshold, 0.25f);
        Set(CTSettingKey.LowHealthHurtLevel2Threshold, 0.12f);
        Set(CTSettingKey.HideFilmgrain, false);
        Set(CTSettingKey.DisableClockTicking, true);
        Set(CTSettingKey.Language, "SFDCT_Default");
        Set(CTSettingKey.SpectatorsMaximum, 4);
        Set(CTSettingKey.SpectatorsOnlyModerators, true);
        Set(CTSettingKey.VoteKickEnabled, false);
        Set(CTSettingKey.VoteKickFailCooldown, 150);
        Set(CTSettingKey.VoteKickSuccessCooldown, 60);
    }

    internal static void LoadDefault()
    {
        ConfigHandler.ReadLine(";If this seems chaotic or shuffled randomly");
        ConfigHandler.ReadLine(";you might want to make a copy of this file,");
        ConfigHandler.ReadLine(";rename it and SFDCT will create a new one,");
        ConfigHandler.ReadLine(";then manually copy your settings to it");

        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningEnabled), true);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningStrength), 0.71f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningForceScreenSpace), false);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningInworldThreshold), 60);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningInworldDistance), 400);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationEnabled), true);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationMin), 0.6f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationForceScreenSpace), false);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationInworldThreshold), 60);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationInworldDistance), 500);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.LowHealthSaturationFactor), 0.71f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.LowHealthThreshold), 0.25f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.LowHealthHurtLevel1Threshold), 0.25f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.LowHealthHurtLevel2Threshold), 0.12f);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.HideFilmgrain), false);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.DisableClockTicking), true);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.Language), "SFDCT_Default");
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SpectatorsMaximum), 4);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.SpectatorsOnlyModerators), true);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.VoteKickEnabled), false);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.VoteKickFailCooldown), 150);
        ConfigHandler.ReadLine(GetSettingKey(CTSettingKey.VoteKickSuccessCooldown), 60);
    }

    internal static string GetSettingKey(CTSettingKey setting)
    {
        switch (setting)
        {
            default: return "UNKNOWN";
            case CTSettingKey.SoundPanningEnabled: return "SOUNDPANNING_ENABLED";
            case CTSettingKey.SoundPanningStrength: return "SOUNDPANNING_STRENGTH";
            case CTSettingKey.SoundPanningForceScreenSpace: return "SOUNDPANNING_FORCE_SCREEN_SPACE";
            case CTSettingKey.SoundPanningInworldThreshold: return "SOUNDPANNING_INWORLD_THRESHOLD";
            case CTSettingKey.SoundPanningInworldDistance: return "SOUNDPANNING_INWORLD_DISTANCE";
            case CTSettingKey.SoundAttenuationEnabled: return "SOUNDATTENUATION_ENABLED";
            case CTSettingKey.SoundAttenuationMin: return "SOUNDATTENUATION_MIN";
            case CTSettingKey.SoundAttenuationForceScreenSpace: return "SOUNDATTENUATION_FORCE_SCREEN_SPACE";
            case CTSettingKey.SoundAttenuationInworldThreshold: return "SOUNDATTENUATION_INWORLD_THRESHOLD";
            case CTSettingKey.SoundAttenuationInworldDistance: return "SOUNDATTENUATION_INWORLD_DISTANCE";
            case CTSettingKey.LowHealthSaturationFactor: return "LOW_HEALTH_SATURATION_FACTOR";
            case CTSettingKey.LowHealthThreshold: return "LOW_HEALTH_THRESHOLD";
            case CTSettingKey.LowHealthHurtLevel1Threshold: return "LOW_HEALTH_HURTLEVEL1_THRESHOLD";
            case CTSettingKey.LowHealthHurtLevel2Threshold: return "LOW_HEALTH_HURTLEVEL2_THRESHOLD";
            case CTSettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
            case CTSettingKey.Language: return "LANGUAGE_FILE_NAME";
            case CTSettingKey.DisableClockTicking: return "DISABLE_CLOCK_TICKING";
            case CTSettingKey.SpectatorsMaximum: return "SPECTATORS_MAXIMUM";
            case CTSettingKey.SpectatorsOnlyModerators: return "SPECTATORS_ONLY_MODERATORS";
            case CTSettingKey.VoteKickEnabled: return "VOTEKICK_ENABLED";
            case CTSettingKey.VoteKickSuccessCooldown: return "VOTEKICK_SUCCESS_COOLDOWN";
            case CTSettingKey.VoteKickFailCooldown: return "VOTEKICK_FAIL_COOLDOWN";
        }
    }

    internal static T Get<T>(CTSettingKey key)
    {
        if (Settings.ContainsKey(key) && typeof(T) == Settings[key].GetType())
        {
            // Logger.LogDebug($"Setting GET: {key}, {typeof(T)}");
            return (T)Settings[key];
        }

        // Logger.LogDebug($"Setting GET: {key}, {typeof(T)} not found!");
        return default;
    }

    internal static void Set<T>(CTSettingKey key, T value)
    {
        // Logger.LogDebug($"Setting SET: {key}, {typeof(T)}: {value}");

        if (!Settings.ContainsKey(key))
        {
            Settings.Add(key, value);
        }
        else
        {
            Settings[key] = value;
        }
    }
}

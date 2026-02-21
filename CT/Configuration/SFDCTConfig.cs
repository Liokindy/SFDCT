using SFD;
using SFDCT.Helper;
using SFDCT.Misc;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SFDCT.Configuration;

internal static class SFDCTConfig
{
    private static readonly Dictionary<CTSettingKey, object> Settings = [];

    internal static void SaveFile()
    {
        var handler = new CTIniHandler();

        SaveFile(handler);

        handler.Dispose();
    }

    internal static void SaveFile(CTIniHandler handler)
    {
        ReadToFile(handler);
        handler.SaveFile(Globals.Paths.ConfigurationIni);
    }

    internal static void LoadFile()
    {
        SetDefault();
        var handler = new CTIniHandler();

        if (!File.Exists(Globals.Paths.ConfigurationIni))
        {
            // Create file and revert to defaults
#if DEBUG
            Logger.LogDebug("Setting CREATING");
#endif

            using (FileStream fileStream = File.Create(Globals.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }

            Thread.Sleep(100);
        }
        else
        {
            // Load file and read settings
#if DEBUG
            Logger.LogDebug("Setting READING FILE");
#endif
            handler.ReadFile(Globals.Paths.ConfigurationIni);

#if DEBUG
            Logger.LogDebug("Setting UPDATING");
#endif
            Set(CTSettingKey.SoundPanningEnabled, handler.ReadValueBool(GetSettingKey(CTSettingKey.SoundPanningEnabled), true));
            Set(CTSettingKey.SoundPanningStrength, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.SoundPanningStrength), 0.71f, 0f, 1f));
            Set(CTSettingKey.SoundPanningForceScreenSpace, handler.ReadValueBool(GetSettingKey(CTSettingKey.SoundPanningForceScreenSpace), false));
            Set(CTSettingKey.SoundPanningInworldThreshold, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundPanningInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundPanningInworldDistance, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundPanningInworldDistance), 400, 0, 1000));
            Set(CTSettingKey.SoundAttenuationEnabled, handler.ReadValueBool(GetSettingKey(CTSettingKey.SoundAttenuationEnabled), true));
            Set(CTSettingKey.SoundAttenuationMin, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.SoundAttenuationMin), 0.6f, 0f, 1f));
            Set(CTSettingKey.SoundAttenuationForceScreenSpace, handler.ReadValueBool(GetSettingKey(CTSettingKey.SoundAttenuationForceScreenSpace), false));
            Set(CTSettingKey.SoundAttenuationInworldThreshold, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundAttenuationInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundAttenuationInworldDistance, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SoundAttenuationInworldDistance), 500, 0, 1000));
            Set(CTSettingKey.LowHealthSaturationFactor, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthSaturationFactor), 0.71f, 0f, 1f));
            Set(CTSettingKey.LowHealthThreshold, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthThreshold), 0.25f, 0f, 1f));
            Set(CTSettingKey.LowHealthHurtLevel1Threshold, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthHurtLevel1Threshold), 0.25f, 0f, 1f));
            Set(CTSettingKey.LowHealthHurtLevel2Threshold, handler.ReadValueFloatCapped(GetSettingKey(CTSettingKey.LowHealthHurtLevel2Threshold), 0.12f, 0f, 1f));
            Set(CTSettingKey.HideFilmgrain, handler.ReadValueBool(GetSettingKey(CTSettingKey.HideFilmgrain), false));
            Set(CTSettingKey.Language, handler.ReadValueString(GetSettingKey(CTSettingKey.Language), "SFDCT_Default"));
            Set(CTSettingKey.SpectatorsMaximum, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.SpectatorsMaximum), 4, 0, 4));
            Set(CTSettingKey.SpectatorsOnlyModerators, handler.ReadValueBool(GetSettingKey(CTSettingKey.SpectatorsOnlyModerators), true));
            Set(CTSettingKey.VoteKickEnabled, handler.ReadValueBool(GetSettingKey(CTSettingKey.VoteKickEnabled), false));
            Set(CTSettingKey.VoteKickFailCooldown, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.VoteKickFailCooldown), 150, 30, 300));
            Set(CTSettingKey.VoteKickSuccessCooldown, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.VoteKickSuccessCooldown), 60, 30, 300));
            Set(CTSettingKey.SubContent, handler.ReadValueBool(GetSettingKey(CTSettingKey.SubContent), true));
            Set(CTSettingKey.SubContentDisabledFolders, handler.ReadValueString(GetSettingKey(CTSettingKey.SubContentDisabledFolders), string.Empty));
            Set(CTSettingKey.SubContentEnabledFolders, handler.ReadValueString(GetSettingKey(CTSettingKey.SubContentEnabledFolders), string.Empty));
            Set(CTSettingKey.ChatWidth, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.ChatWidth), 428, 428 / 2, 428 * 4));
            Set(CTSettingKey.ChatHeight, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.ChatHeight), 10 * (int)GameChat.MESSAGE_HEIGHT, 10 * (int)GameChat.MESSAGE_HEIGHT / 2, 10 * (int)GameChat.MESSAGE_HEIGHT * 4));
            Set(CTSettingKey.ChatExtraHeight, handler.ReadValueIntCapped(GetSettingKey(CTSettingKey.ChatExtraHeight), 0, 0, 10 * (int)GameChat.MESSAGE_HEIGHT * 2));
        }

        ReadToFile(handler);
        SaveFile(handler);

        handler.Dispose();
    }

    internal static void SetDefault()
    {
#if DEBUG
        Logger.LogDebug("Setting REVERING TO DEFAULTS");
#endif

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
        Set(CTSettingKey.Language, "SFDCT_Default");
        Set(CTSettingKey.SpectatorsMaximum, 4);
        Set(CTSettingKey.SpectatorsOnlyModerators, true);
        Set(CTSettingKey.VoteKickEnabled, false);
        Set(CTSettingKey.VoteKickFailCooldown, 150);
        Set(CTSettingKey.VoteKickSuccessCooldown, 60);
        Set(CTSettingKey.SubContent, true);
        Set(CTSettingKey.SubContentDisabledFolders, string.Empty);
        Set(CTSettingKey.SubContentEnabledFolders, string.Empty);
        Set(CTSettingKey.ChatWidth, 428);
        Set(CTSettingKey.ChatHeight, 10 * (int)GameChat.MESSAGE_HEIGHT);
        Set(CTSettingKey.ChatExtraHeight, 0);
    }

    internal static void ReadToFile(CTIniHandler handler)
    {
        handler.Clear();
        handler.ReadLine(";If this seems chaotic or shuffled randomly");
        handler.ReadLine(";you might want to make a copy of this file,");
        handler.ReadLine(";rename it and SFDCT will create a new one,");
        handler.ReadLine(";then manually copy your settings to it");

        handler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningEnabled), Get<bool>(CTSettingKey.SoundPanningEnabled));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningStrength), Get<float>(CTSettingKey.SoundPanningStrength));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningForceScreenSpace), Get<bool>(CTSettingKey.SoundPanningForceScreenSpace));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningInworldThreshold), Get<int>(CTSettingKey.SoundPanningInworldThreshold));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundPanningInworldDistance), Get<int>(CTSettingKey.SoundPanningInworldDistance));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationEnabled), Get<bool>(CTSettingKey.SoundAttenuationEnabled));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationMin), Get<float>(CTSettingKey.SoundAttenuationMin));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationForceScreenSpace), Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationInworldThreshold), Get<int>(CTSettingKey.SoundAttenuationInworldThreshold));
        handler.ReadLine(GetSettingKey(CTSettingKey.SoundAttenuationInworldDistance), Get<int>(CTSettingKey.SoundAttenuationInworldDistance));
        handler.ReadLine(GetSettingKey(CTSettingKey.LowHealthSaturationFactor), Get<float>(CTSettingKey.LowHealthSaturationFactor));
        handler.ReadLine(GetSettingKey(CTSettingKey.LowHealthThreshold), Get<float>(CTSettingKey.LowHealthThreshold));
        handler.ReadLine(GetSettingKey(CTSettingKey.LowHealthHurtLevel1Threshold), Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold));
        handler.ReadLine(GetSettingKey(CTSettingKey.LowHealthHurtLevel2Threshold), Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold));
        handler.ReadLine(GetSettingKey(CTSettingKey.HideFilmgrain), Get<bool>(CTSettingKey.HideFilmgrain));
        handler.ReadLine(GetSettingKey(CTSettingKey.Language), Get<string>(CTSettingKey.Language));
        handler.ReadLine(GetSettingKey(CTSettingKey.SpectatorsMaximum), Get<int>(CTSettingKey.SpectatorsMaximum));
        handler.ReadLine(GetSettingKey(CTSettingKey.SpectatorsOnlyModerators), Get<bool>(CTSettingKey.SpectatorsOnlyModerators));
        handler.ReadLine(GetSettingKey(CTSettingKey.VoteKickEnabled), Get<bool>(CTSettingKey.VoteKickEnabled));
        handler.ReadLine(GetSettingKey(CTSettingKey.VoteKickFailCooldown), Get<int>(CTSettingKey.VoteKickFailCooldown));
        handler.ReadLine(GetSettingKey(CTSettingKey.VoteKickSuccessCooldown), Get<int>(CTSettingKey.VoteKickSuccessCooldown));
        handler.ReadLine(GetSettingKey(CTSettingKey.SubContent), Get<bool>(CTSettingKey.SubContent));
        handler.ReadLine(GetSettingKey(CTSettingKey.SubContentDisabledFolders), Get<string>(CTSettingKey.SubContentDisabledFolders));
        handler.ReadLine(GetSettingKey(CTSettingKey.SubContentEnabledFolders), Get<string>(CTSettingKey.SubContentEnabledFolders));
        handler.ReadLine(GetSettingKey(CTSettingKey.ChatWidth), Get<int>(CTSettingKey.ChatWidth));
        handler.ReadLine(GetSettingKey(CTSettingKey.ChatHeight), Get<int>(CTSettingKey.ChatHeight));
        handler.ReadLine(GetSettingKey(CTSettingKey.ChatExtraHeight), Get<int>(CTSettingKey.ChatExtraHeight));
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
            case CTSettingKey.SpectatorsMaximum: return "SPECTATORS_MAXIMUM";
            case CTSettingKey.SpectatorsOnlyModerators: return "SPECTATORS_ONLY_MODERATORS";
            case CTSettingKey.VoteKickEnabled: return "VOTEKICK_ENABLED";
            case CTSettingKey.VoteKickSuccessCooldown: return "VOTEKICK_SUCCESS_COOLDOWN";
            case CTSettingKey.VoteKickFailCooldown: return "VOTEKICK_FAIL_COOLDOWN";
            case CTSettingKey.SubContent: return "SUBCONTENT";
            case CTSettingKey.SubContentDisabledFolders: return "SUBCONTENT_DISABLED_FOLDERS";
            case CTSettingKey.SubContentEnabledFolders: return "SUBCONTENT_ENABLED_FOLDERS";
            case CTSettingKey.ChatWidth: return "CHAT_WIDTH";
            case CTSettingKey.ChatHeight: return "CHAT_HEIGHT";
            case CTSettingKey.ChatExtraHeight: return "CHAT_EXTRA_HEIGHT";
        }
    }

    internal static T Get<T>(CTSettingKey key)
    {
        if (Settings.ContainsKey(key) && typeof(T) == Settings[key].GetType())
        {
            return (T)Settings[key];
        }

        return default;
    }

    internal static void Set<T>(CTSettingKey key, T value)
    {
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

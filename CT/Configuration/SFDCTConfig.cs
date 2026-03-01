using SFD;
using SFDCT.Misc;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SFDCT.Configuration;

internal static class SFDCTConfig
{
    private static readonly Dictionary<CTSettingKey, object> Settings = [];

    internal static void Save()
    {
        var handler = new CTIniHandler();

        Save(handler);

        handler.Dispose();
    }

    internal static void Save(CTIniHandler handler)
    {
        if (ConsoleOutput.m_texts[0] != null) ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "SFDCT: Reading settings to configuration file");

        SetSettingsToFile(handler);

        if (ConsoleOutput.m_texts[0] != null) ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "SFDCT: Saving configuration file");
        handler.SaveFile(Globals.Paths.ConfigurationIni);
    }

    internal static void Load()
    {
        SetSettingsToDefaults();
        var handler = new CTIniHandler();

        if (!File.Exists(Globals.Paths.ConfigurationIni))
        {
            if (ConsoleOutput.m_texts[0] != null) ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "SFDCT: Creating configuration file");

            using (FileStream fileStream = File.Create(Globals.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }

            Thread.Sleep(100);
        }
        else
        {
            if (ConsoleOutput.m_texts[0] != null) ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "SFDCT: Reading configuration file");

            handler.ReadFile(Globals.Paths.ConfigurationIni);
            Set(CTSettingKey.SoundPanningEnabled, handler.ReadValueBool(GetKey(CTSettingKey.SoundPanningEnabled), true));
            Set(CTSettingKey.SoundPanningStrength, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundPanningStrength), 70, 0, 100) * 0.01f);
            Set(CTSettingKey.SoundPanningForceScreenSpace, handler.ReadValueBool(GetKey(CTSettingKey.SoundPanningForceScreenSpace), false));
            Set(CTSettingKey.SoundPanningInworldThreshold, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundPanningInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundPanningInworldDistance, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundPanningInworldDistance), 400, 0, 1000));
            Set(CTSettingKey.SoundAttenuationEnabled, handler.ReadValueBool(GetKey(CTSettingKey.SoundAttenuationEnabled), true));
            Set(CTSettingKey.SoundAttenuationMin, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundAttenuationMin), 60, 0, 100) * 0.01f);
            Set(CTSettingKey.SoundAttenuationForceScreenSpace, handler.ReadValueBool(GetKey(CTSettingKey.SoundAttenuationForceScreenSpace), false));
            Set(CTSettingKey.SoundAttenuationInworldThreshold, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundAttenuationInworldThreshold), 60, 0, 1000));
            Set(CTSettingKey.SoundAttenuationInworldDistance, handler.ReadValueIntCapped(GetKey(CTSettingKey.SoundAttenuationInworldDistance), 500, 0, 1000));
            Set(CTSettingKey.LowHealthSaturationFactor, handler.ReadValueIntCapped(GetKey(CTSettingKey.LowHealthSaturationFactor), 70, 0, 100) * 0.01f);
            Set(CTSettingKey.LowHealthThreshold, handler.ReadValueIntCapped(GetKey(CTSettingKey.LowHealthThreshold), 25, 0, 100) * 0.01f);
            Set(CTSettingKey.LowHealthHurtLevel1Threshold, handler.ReadValueIntCapped(GetKey(CTSettingKey.LowHealthHurtLevel1Threshold), 25, 0, 100) * 0.01f);
            Set(CTSettingKey.LowHealthHurtLevel2Threshold, handler.ReadValueIntCapped(GetKey(CTSettingKey.LowHealthHurtLevel2Threshold), 12, 0, 100) * 0.01f);
            Set(CTSettingKey.HideFilmgrain, handler.ReadValueBool(GetKey(CTSettingKey.HideFilmgrain), false));
            Set(CTSettingKey.Language, handler.ReadValueString(GetKey(CTSettingKey.Language), "SFDCT_Default"));
            Set(CTSettingKey.SpectatorsMaximum, handler.ReadValueIntCapped(GetKey(CTSettingKey.SpectatorsMaximum), 4, 0, 4));
            Set(CTSettingKey.SpectatorsOnlyModerators, handler.ReadValueBool(GetKey(CTSettingKey.SpectatorsOnlyModerators), true));
            Set(CTSettingKey.VoteKickEnabled, handler.ReadValueBool(GetKey(CTSettingKey.VoteKickEnabled), false));
            Set(CTSettingKey.VoteKickFailCooldown, handler.ReadValueIntCapped(GetKey(CTSettingKey.VoteKickFailCooldown), 150, 15, 300));
            Set(CTSettingKey.VoteKickSuccessCooldown, handler.ReadValueIntCapped(GetKey(CTSettingKey.VoteKickSuccessCooldown), 60, 15, 300));
            Set(CTSettingKey.SubContent, handler.ReadValueBool(GetKey(CTSettingKey.SubContent), true));
            Set(CTSettingKey.SubContentDisabledFolders, handler.ReadValueString(GetKey(CTSettingKey.SubContentDisabledFolders), string.Empty));
            Set(CTSettingKey.SubContentEnabledFolders, handler.ReadValueString(GetKey(CTSettingKey.SubContentEnabledFolders), string.Empty));
            Set(CTSettingKey.ChatWidth, handler.ReadValueIntCapped(GetKey(CTSettingKey.ChatWidth), 428, 428 / 2, 428 * 4));
            Set(CTSettingKey.ChatHeight, handler.ReadValueIntCapped(GetKey(CTSettingKey.ChatHeight), 10 * (int)GameChat.MESSAGE_HEIGHT, 10 * (int)GameChat.MESSAGE_HEIGHT / 2, 10 * (int)GameChat.MESSAGE_HEIGHT * 4));
            Set(CTSettingKey.ChatExtraHeight, handler.ReadValueIntCapped(GetKey(CTSettingKey.ChatExtraHeight), 0, 0, 10 * (int)GameChat.MESSAGE_HEIGHT * 2));
            Set(CTSettingKey.ChatIndependentTeamRandomColors, handler.ReadValueBool(GetKey(CTSettingKey.ChatIndependentTeamRandomColors), true));
            Set(CTSettingKey.ExtraAccountDataChecking, handler.ReadValueBool(GetKey(CTSettingKey.ExtraAccountDataChecking), true));
            Set(CTSettingKey.LogConsoleOutput, handler.ReadValueBool(GetKey(CTSettingKey.LogConsoleOutput), false));
            Set(CTSettingKey.LogConsoleOutputFolder, handler.ReadValueString(GetKey(CTSettingKey.LogConsoleOutputFolder), ""));
        }

        SetSettingsToFile(handler);
        Save(handler);

        handler.Dispose();
    }

    private static void SetSettingsToDefaults()
    {
        Set(CTSettingKey.SoundPanningEnabled, true);
        Set(CTSettingKey.SoundPanningStrength, 0.70f);
        Set(CTSettingKey.SoundPanningForceScreenSpace, false);
        Set(CTSettingKey.SoundPanningInworldThreshold, 60);
        Set(CTSettingKey.SoundPanningInworldDistance, 400);
        Set(CTSettingKey.SoundAttenuationEnabled, true);
        Set(CTSettingKey.SoundAttenuationMin, 0.6f);
        Set(CTSettingKey.SoundAttenuationForceScreenSpace, false);
        Set(CTSettingKey.SoundAttenuationInworldThreshold, 60);
        Set(CTSettingKey.SoundAttenuationInworldDistance, 500);
        Set(CTSettingKey.LowHealthSaturationFactor, 0.70f);
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
        Set(CTSettingKey.ChatIndependentTeamRandomColors, true);
        Set(CTSettingKey.ExtraAccountDataChecking, true);
        Set(CTSettingKey.LogConsoleOutput, false);
        Set(CTSettingKey.LogConsoleOutputFolder, "");
    }

    private static void SetSettingsToFile(CTIniHandler handler)
    {
        handler.Clear();
        handler.ReadLine(";If this seems chaotic or shuffled randomly");
        handler.ReadLine(";you might want to make a copy of this file,");
        handler.ReadLine(";rename it and SFDCT will create a new one,");
        handler.ReadLine(";then manually copy your settings to it");

        handler.ReadLine(GetKey(CTSettingKey.SoundPanningEnabled), Get<bool>(CTSettingKey.SoundPanningEnabled));
        handler.ReadLine(GetKey(CTSettingKey.SoundPanningStrength), (int)(Get<float>(CTSettingKey.SoundPanningStrength) * 100));
        handler.ReadLine(GetKey(CTSettingKey.SoundPanningForceScreenSpace), Get<bool>(CTSettingKey.SoundPanningForceScreenSpace));
        handler.ReadLine(GetKey(CTSettingKey.SoundPanningInworldThreshold), Get<int>(CTSettingKey.SoundPanningInworldThreshold));
        handler.ReadLine(GetKey(CTSettingKey.SoundPanningInworldDistance), Get<int>(CTSettingKey.SoundPanningInworldDistance));
        handler.ReadLine(GetKey(CTSettingKey.SoundAttenuationEnabled), Get<bool>(CTSettingKey.SoundAttenuationEnabled));
        handler.ReadLine(GetKey(CTSettingKey.SoundAttenuationMin), (int)(Get<float>(CTSettingKey.SoundAttenuationMin) * 100));
        handler.ReadLine(GetKey(CTSettingKey.SoundAttenuationForceScreenSpace), Get<bool>(CTSettingKey.SoundAttenuationForceScreenSpace));
        handler.ReadLine(GetKey(CTSettingKey.SoundAttenuationInworldThreshold), Get<int>(CTSettingKey.SoundAttenuationInworldThreshold));
        handler.ReadLine(GetKey(CTSettingKey.SoundAttenuationInworldDistance), Get<int>(CTSettingKey.SoundAttenuationInworldDistance));
        handler.ReadLine(GetKey(CTSettingKey.LowHealthSaturationFactor), (int)(Get<float>(CTSettingKey.LowHealthSaturationFactor) * 100));
        handler.ReadLine(GetKey(CTSettingKey.LowHealthThreshold), (int)(Get<float>(CTSettingKey.LowHealthThreshold) * 100));
        handler.ReadLine(GetKey(CTSettingKey.LowHealthHurtLevel1Threshold), (int)(Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold) * 100));
        handler.ReadLine(GetKey(CTSettingKey.LowHealthHurtLevel2Threshold), (int)(Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold) * 100));
        handler.ReadLine(GetKey(CTSettingKey.HideFilmgrain), Get<bool>(CTSettingKey.HideFilmgrain));
        handler.ReadLine(GetKey(CTSettingKey.Language), Get<string>(CTSettingKey.Language));
        handler.ReadLine(GetKey(CTSettingKey.SpectatorsMaximum), Get<int>(CTSettingKey.SpectatorsMaximum));
        handler.ReadLine(GetKey(CTSettingKey.SpectatorsOnlyModerators), Get<bool>(CTSettingKey.SpectatorsOnlyModerators));
        handler.ReadLine(GetKey(CTSettingKey.VoteKickEnabled), Get<bool>(CTSettingKey.VoteKickEnabled));
        handler.ReadLine(GetKey(CTSettingKey.VoteKickFailCooldown), Get<int>(CTSettingKey.VoteKickFailCooldown));
        handler.ReadLine(GetKey(CTSettingKey.VoteKickSuccessCooldown), Get<int>(CTSettingKey.VoteKickSuccessCooldown));
        handler.ReadLine(GetKey(CTSettingKey.SubContent), Get<bool>(CTSettingKey.SubContent));
        handler.ReadLine(GetKey(CTSettingKey.SubContentDisabledFolders), Get<string>(CTSettingKey.SubContentDisabledFolders));
        handler.ReadLine(GetKey(CTSettingKey.SubContentEnabledFolders), Get<string>(CTSettingKey.SubContentEnabledFolders));
        handler.ReadLine(GetKey(CTSettingKey.ChatWidth), Get<int>(CTSettingKey.ChatWidth));
        handler.ReadLine(GetKey(CTSettingKey.ChatHeight), Get<int>(CTSettingKey.ChatHeight));
        handler.ReadLine(GetKey(CTSettingKey.ChatExtraHeight), Get<int>(CTSettingKey.ChatExtraHeight));
        handler.ReadLine(GetKey(CTSettingKey.ChatIndependentTeamRandomColors), Get<bool>(CTSettingKey.ChatIndependentTeamRandomColors));
        handler.ReadLine(GetKey(CTSettingKey.ExtraAccountDataChecking), Get<bool>(CTSettingKey.ExtraAccountDataChecking));
        handler.ReadLine(GetKey(CTSettingKey.LogConsoleOutput), Get<bool>(CTSettingKey.LogConsoleOutput));
        handler.ReadLine(GetKey(CTSettingKey.LogConsoleOutputFolder), Get<string>(CTSettingKey.LogConsoleOutputFolder));
    }

    internal static string GetKey(CTSettingKey setting)
    {
        switch (setting)
        {
            default: return "UNKNOWN_" + (int)setting;
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
            case CTSettingKey.ChatIndependentTeamRandomColors: return "CHAT_TEAM_INDEPENDENT_RANDOM_NAME_COLOR";
            case CTSettingKey.ExtraAccountDataChecking: return "EXTRA_ACCOUNT_DATA_CHECKING";
            case CTSettingKey.LogConsoleOutput: return "LOG_CONSOLE";
            case CTSettingKey.LogConsoleOutputFolder: return "LOG_CONSOLE_FOLDER";
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

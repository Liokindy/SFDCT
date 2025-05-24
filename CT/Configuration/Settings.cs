using SFDCT.Helper;
using System.Collections.Generic;

namespace SFDCT.Configuration;

public static class Settings
{
    public static Dictionary<string, IniSetting> List { get { return new(m_list); } }

    private static Dictionary<string, IniSetting> m_list = [];
    private static bool b_initialized = false;

    public static void Init()
    {
        if (b_initialized)
        {
            return;
        }

        Add(SettingKey.SoundPanningEnabled, IniSettingType.Bool, true);
        Add(SettingKey.SoundPanningStrength, IniSettingType.Float, 0.71f, false, 0f, 1f);
        Add(SettingKey.SoundPanningForceScreenSpace, IniSettingType.Bool, false);
        Add(SettingKey.SoundPanningInworldThreshold, IniSettingType.Int, 60, false, 0, 1000);
        Add(SettingKey.SoundPanningInworldDistance, IniSettingType.Int, 400, false, 0, 1000);
        Add(SettingKey.SoundAttenuationEnabled, IniSettingType.Bool, true);
        Add(SettingKey.SoundAttenuationMin, IniSettingType.Float, 0.6f, false, 0f, 1f);
        Add(SettingKey.SoundAttenuationForceScreenSpace, IniSettingType.Bool, false);
        Add(SettingKey.SoundAttenuationInworldThreshold, IniSettingType.Int, 60, false, 0, 1000);
        Add(SettingKey.SoundAttenuationInworldDistance, IniSettingType.Int, 500, false, 0, 1000);
        Add(SettingKey.LowHealthSaturationFactor, IniSettingType.Float, 0.71f, false, 0f, 1f);
        Add(SettingKey.LowHealthThreshold, IniSettingType.Float, 0.25f, false, 0f, 1f);
        Add(SettingKey.LowHealthHurtLevel1Threshold, IniSettingType.Float, 0.25f, true, 0f, 1f);
        Add(SettingKey.LowHealthHurtLevel2Threshold, IniSettingType.Float, 0.12f, true, 0f, 1f);
        Add(SettingKey.HideFilmgrain, IniSettingType.Bool, false);
        Add(SettingKey.DisableClockTicking, IniSettingType.Bool, true);
        Add(SettingKey.Language, IniSettingType.String, "SFDCT_default", false);
        Add(SettingKey.SpectatorsMaximum, IniSettingType.Int, 4, false, 0, 4);
        Add(SettingKey.SpectatorsOnlyModerators, IniSettingType.Bool, true);
        Add(SettingKey.VoteKickEnabled, IniSettingType.Bool, false);
        Add(SettingKey.VoteKickFailCooldown, IniSettingType.Int, 150, false, 30, 300);
        Add(SettingKey.VoteKickSuccessCooldown, IniSettingType.Int, 60, false, 30, 300);

        b_initialized = true;
    }

    private static void Add(SettingKey settingKey, IniSettingType type, object value, bool requiresRestart = false, object minValue = null, object maxValue = null)
    {
        Add(GetKey(settingKey), type, value, requiresRestart, minValue, maxValue, GetName(settingKey), GetHelp(settingKey), GetCategoryName(settingKey));
    }
    private static void Add(string stringKey, IniSettingType type, object value, bool requiresRestart = false, object minValue = null, object maxValue = null, string settingName = "", string settingHelp = "", string settingCategory = "")
    {
        m_list.Add(stringKey.ToUpperInvariant(), new IniSetting(stringKey, type, value, requiresRestart, minValue, maxValue, settingName, settingHelp, settingCategory));
    }

    public static T Get<T>(SettingKey key, bool getDefault = false)
    {
        return Get<T>(GetKey(key), getDefault);
    }

    public static T Get<T>(string key, bool getDefault = false)
    {
        key = key.ToUpperInvariant();

        if (!m_list.ContainsKey(key))
        {
            string mess = $"CONFIG.INI: Trying to GET key '{key}', key not found!!";
            Logger.LogError(mess);
            throw new KeyNotFoundException(mess);
        }

        IniSetting setting = m_list[key];

        return (T)setting.Get(getDefault);
    }

    public static void Set<T>(SettingKey key, T value)
    {
        Set<T>(GetKey(key), value);
    }

    public static void Set<T>(string key, T value)
    {
        key = key.ToUpperInvariant();

        if (!m_list.ContainsKey(key))
        {
            string mess = $"CONFIG.INI: Trying to SET key '{key}', key not found!!";
            Logger.LogError(mess);
            throw new KeyNotFoundException(mess);
        }

        IniSetting setting = m_list[key];

        if (setting.RequiresGameRestart && !IniFile.FirstRefresh && !setting.Value.Equals(value))
        {
            string messHeader = "CONFIG.INI:";
            string mess = $"'{key}' requires GAME-RESTART to change from {setting.Value} to {value}!";
            Logger.LogWarn(messHeader + mess);

            SFD.MessageStack.Show(mess, SFD.MessageStackType.Warning);
        }

        if (!setting.Value.Equals(value))
        {
            IniFile.NeedsSaving = true;
        }

        setting.Value = value;
    }

    //      Not required, but it is very convenient as it provides
    //      an easy way of changing the setting keys, names or help if needed
    public static string GetKey(SettingKey key)
    {
        switch (key)
        {
            default: return "UNKNOWN";
            case SettingKey.SoundPanningEnabled: return "SOUNDPANNING_ENABLED";
            case SettingKey.SoundPanningStrength: return "SOUNDPANNING_STRENGTH";
            case SettingKey.SoundPanningForceScreenSpace: return "SOUNDPANNING_FORCE_SCREEN_SPACE";
            case SettingKey.SoundPanningInworldThreshold: return "SOUNDPANNING_INWORLD_THRESHOLD";
            case SettingKey.SoundPanningInworldDistance: return "SOUNDPANNING_INWORLD_DISTANCE";
            case SettingKey.SoundAttenuationEnabled: return "SOUNDATTENUATION_ENABLED";
            case SettingKey.SoundAttenuationMin: return "SOUNDATTENUATION_MIN";
            case SettingKey.SoundAttenuationForceScreenSpace: return "SOUNDATTENUATION_FORCE_SCREEN_SPACE";
            case SettingKey.SoundAttenuationInworldThreshold: return "SOUNDATTENUATION_INWORLD_THRESHOLD";
            case SettingKey.SoundAttenuationInworldDistance: return "SOUNDATTENUATION_INWORLD_DISTANCE";
            case SettingKey.LowHealthSaturationFactor: return "LOW_HEALTH_SATURATION_FACTOR";
            case SettingKey.LowHealthThreshold: return "LOW_HEALTH_THRESHOLD";
            case SettingKey.LowHealthHurtLevel1Threshold: return "LOW_HEALTH_HURTLEVEL1_THRESHOLD";
            case SettingKey.LowHealthHurtLevel2Threshold: return "LOW_HEALTH_HURTLEVEL2_THRESHOLD";
            case SettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
            case SettingKey.Language: return "LANGUAGE_FILE_NAME";
            case SettingKey.DisableClockTicking: return "DISABLE_CLOCK_TICKING";
            case SettingKey.SpectatorsMaximum: return "SPECTATORS_MAXIMUM";
            case SettingKey.SpectatorsOnlyModerators: return "SPECTATORS_ONLY_MODERATORS";
            case SettingKey.VoteKickEnabled: return "VOTEKICK_ENABLED";
            case SettingKey.VoteKickSuccessCooldown: return "VOTEKICK_SUCCESS_COOLDOWN";
            case SettingKey.VoteKickFailCooldown: return "VOTEKICK_FAIL_COOLDOWN";
        }
    }

    public static string GetName(SettingKey key)
    {
        return key.ToString().ToLowerInvariant();
    }

    public static string GetHelp(SettingKey key)
    {
        return key.ToString().ToLowerInvariant();
    }

    public static string GetCategoryName(SettingKey key)
    {
        return key.ToString().ToLowerInvariant();
    }
}
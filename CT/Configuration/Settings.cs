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
        Add(SettingKey.SpectatorsMaximum, IniSettingType.Int, 4, false, 0, 4);
        Add(SettingKey.SpectatorsOnlyModerators, IniSettingType.Bool, true);
        Add(SettingKey.VoteKickEnabled, IniSettingType.Bool, true);
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

        switch (key)
        {
            default: return "Unknown-Setting";
            case SettingKey.SoundPanningEnabled: return "Enabled";
            case SettingKey.SoundPanningStrength: return "Strength";
            case SettingKey.SoundPanningForceScreenSpace: return "Force Screen-Space";
            case SettingKey.SoundPanningInworldThreshold: return "Threshold";
            case SettingKey.SoundPanningInworldDistance: return "Distance";
            case SettingKey.SoundAttenuationEnabled: return "Enabled";
            case SettingKey.SoundAttenuationMin: return "Minimum";
            case SettingKey.SoundAttenuationForceScreenSpace: return "Force Screen-Space";
            case SettingKey.SoundAttenuationInworldThreshold: return "Threshold";
            case SettingKey.SoundAttenuationInworldDistance: return "Distance";
            case SettingKey.LowHealthSaturationFactor: return "Desaturation Strength";
            case SettingKey.LowHealthThreshold: return "Desaturation Threshold";
            case SettingKey.LowHealthHurtLevel1Threshold: return "HurtLevel1 Threshold";
            case SettingKey.LowHealthHurtLevel2Threshold: return "HurtLevel2 Threshold";
            case SettingKey.HideFilmgrain: return "Hide FilmGrain";
            case SettingKey.DisableClockTicking: return "Disable ClockTicking";
            case SettingKey.SpectatorsMaximum: return "Maximum";
            case SettingKey.SpectatorsOnlyModerators: return "Only Moderators";
            case SettingKey.VoteKickEnabled: return "Enabled";
            case SettingKey.VoteKickSuccessCooldown: return "Success cooldown";
            case SettingKey.VoteKickFailCooldown: return "Fail cooldown";
        }
    }

    public static string GetHelp(SettingKey key)
    {
        switch (key)
        {
            default: return "Unknown";
            case SettingKey.SoundPanningEnabled: return "Enables or disables sound-panning";
            case SettingKey.SoundPanningStrength: return "Controls the maximum panning of sounds";
            case SettingKey.SoundPanningForceScreenSpace: return "Controls if sound-panning is calculated using screen-space positions instead of world distances";
            case SettingKey.SoundPanningInworldThreshold: return "Controls the threshold of sound-panning, sounds closer than this will not be panned";
            case SettingKey.SoundPanningInworldDistance: return "Controls the distance of sound-panning, sounds further than this will be fully panned";
            case SettingKey.SoundAttenuationEnabled: return "Enables or disables sound-attenuation";
            case SettingKey.SoundAttenuationMin: return "Controls the minimum volume of sound-attenuation, sounds volume will be lowered to this amount at max attenuation";
            case SettingKey.SoundAttenuationForceScreenSpace: return "Controls if sound-attenuation is calculated using screen-space positions instead of world distances";
            case SettingKey.SoundAttenuationInworldThreshold: return "Controls the threshold of sound-attenuation, sounds closer than this will not be attenuated";
            case SettingKey.SoundAttenuationInworldDistance: return "Controls the distance of sound-attenuation, sounds further than this will be fully attenuated";
            case SettingKey.LowHealthSaturationFactor: return "Controls the desaturation strength";
            case SettingKey.LowHealthThreshold: return "Controls the desaturation threshold";
            case SettingKey.LowHealthHurtLevel1Threshold: return "Controls the threshold for HurtLevel1 on fighters";
            case SettingKey.LowHealthHurtLevel2Threshold: return "Controls the threshold for HurtLevel2 on fighters";
            case SettingKey.HideFilmgrain: return "Forcefully hides the FilmGrain even if Effect Level is set to Normal";
            case SettingKey.DisableClockTicking: return "Disables a 10% random chance for 'ClockTicking' to play as the game loads on startup";
            case SettingKey.SpectatorsMaximum: return "Controls the maximum amount of spectators";
            case SettingKey.SpectatorsOnlyModerators: return "Controls if only moderators and the host can turn into spectators";
            case SettingKey.VoteKickEnabled: return "Enables or disables vote-kicking";
            case SettingKey.VoteKickSuccessCooldown: return "Controls the cooldown in seconds of vote-kicking on success";
            case SettingKey.VoteKickFailCooldown: return "Controls the cooldown in seconds of vote-kicking on failure";
        }
    }

    public static string GetCategoryName(SettingKey key)
    {
        switch (key)
        {
            default: return "UNKNOWN";
            case SettingKey.SoundPanningEnabled:
            case SettingKey.SoundPanningStrength:
            case SettingKey.SoundPanningForceScreenSpace:
            case SettingKey.SoundPanningInworldThreshold:
            case SettingKey.SoundPanningInworldDistance:
                return "SOUND-PANNING";
            case SettingKey.SoundAttenuationEnabled:
            case SettingKey.SoundAttenuationMin:
            case SettingKey.SoundAttenuationForceScreenSpace:
            case SettingKey.SoundAttenuationInworldThreshold:
            case SettingKey.SoundAttenuationInworldDistance:
                return "SOUND-ATTENUATION";
            case SettingKey.LowHealthSaturationFactor:
            case SettingKey.LowHealthThreshold:
            case SettingKey.LowHealthHurtLevel1Threshold:
            case SettingKey.LowHealthHurtLevel2Threshold:
                return "LOW HEALTH";
            case SettingKey.SpectatorsMaximum:
            case SettingKey.SpectatorsOnlyModerators:
                return "SPECTATORS";
            case SettingKey.VoteKickEnabled:
            case SettingKey.VoteKickFailCooldown:
            case SettingKey.VoteKickSuccessCooldown:
                return "VOTE-KICK";
            case SettingKey.HideFilmgrain:
            case SettingKey.DisableClockTicking:
                return "MISC";
        }
    }
}
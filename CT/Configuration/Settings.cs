using System;
using System.Collections.Generic;
using SFDCT.Helper;
using SFDGameScriptInterface;

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

        Add(SettingKey.SoundPanningEnabled,                 IniSettingType.Bool,    true);
        Add(SettingKey.SoundPanningStrength,                IniSettingType.Float,   0.71f,  false, 0f, 1f);
        Add(SettingKey.SoundPanningForceScreenSpace,        IniSettingType.Bool,    false);
        Add(SettingKey.SoundPanningInworldThreshold,        IniSettingType.Int,     60,     false, 0, 1000);
        Add(SettingKey.SoundPanningInworldDistance,         IniSettingType.Int,     400,    false, 0, 1000);
        Add(SettingKey.SoundAttenuationEnabled,             IniSettingType.Bool,    true);
        Add(SettingKey.SoundAttenuationMin,                 IniSettingType.Float,   0.6f,   false, 0f, 1f);
        Add(SettingKey.SoundAttenuationForceScreenSpace,    IniSettingType.Bool,    false);
        Add(SettingKey.SoundAttenuationInworldThreshold,    IniSettingType.Int,     60,     false, 0, 1000);
        Add(SettingKey.SoundAttenuationInworldDistance,     IniSettingType.Int,     500,    false, 0, 1000);
        Add(SettingKey.LowHealthSaturationFactor,           IniSettingType.Float,   0.71f,  false, 0f, 1f);
        Add(SettingKey.LowHealthThreshold,                  IniSettingType.Float,   0.25f,  false, 0f, 1f);
        Add(SettingKey.HideFilmgrain,                       IniSettingType.Bool,    false);

        b_initialized = true;
    }

    private static void Add(SettingKey settingKey, IniSettingType type, object value, bool requiresRestart = false, object minValue = null, object maxValue = null)
    {
        Add(GetKey(settingKey), type, value, requiresRestart, minValue, maxValue, GetName(settingKey), GetHelp(settingKey));
    }
    private static void Add(string stringKey, IniSettingType type, object value, bool requiresRestart = false, object minValue = null, object maxValue = null, string settingName = "", string settingHelp = "")
    {
        m_list.Add(stringKey.ToUpperInvariant(), new IniSetting(stringKey, type, value, requiresRestart, minValue, maxValue, settingName, settingHelp));
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
        if (setting.ValueType != typeof(T))
        {
            string mess = $"CONFIG.INI: Trying to GET value '{key}' from type '{typeof(T)}', key is type {setting.ValueType}!!";
            Logger.LogError(mess);
            throw new Exception(mess);
        }

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
        if (setting.ValueType != typeof(T))
        {
            string mess = $"CONFIG.INI: Trying to SET value '{key}' from type '{typeof(T)}', key is type {setting.ValueType}!!";
            Logger.LogError(mess);
            throw new Exception(mess);
        }

        if (setting.RequiresGameRestart && !IniFile.FirstRefresh)
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
            case SettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
        }
    }

    // TODO:
    public static string GetName(SettingKey key)
    {

        switch (key)
        {
            default: return "Unknown-Setting";
            case SettingKey.SoundPanningEnabled: return "Sound-panning Enabled";
            case SettingKey.SoundPanningStrength: return "Sound-panning Strength";
            case SettingKey.SoundPanningForceScreenSpace: return "Sound-panning force Screen-Space";
            case SettingKey.SoundPanningInworldThreshold: return "Sound-panning Threshold";
            case SettingKey.SoundPanningInworldDistance: return "Sound-panning Distance";
            case SettingKey.SoundAttenuationEnabled: return "Sound-attenuation Enabled";
            case SettingKey.SoundAttenuationMin: return "Sound-attenuation Minimum";
            case SettingKey.SoundAttenuationForceScreenSpace: return "Sound-attenuation force Screen-Space";
            case SettingKey.SoundAttenuationInworldThreshold: return "Sound-attenuation Threshold";
            case SettingKey.SoundAttenuationInworldDistance: return "Sound-attenuation Distance";
            case SettingKey.LowHealthSaturationFactor: return "Low HP Desaturation Factor";
            case SettingKey.LowHealthThreshold: return "Low HP Desaturation Threshold";
            case SettingKey.HideFilmgrain: return "Hide FilmGrain";
        }
    }

    // TODO:
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
            case SettingKey.HideFilmgrain: return "Forcefully hides the FilmGrain even if Effect Level is set to Normal";
        }
    }
}
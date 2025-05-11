using System;
using System.Collections.Generic;
using SFDCT.Helper;

namespace SFDCT.Configuration;

public static partial class Settings
{
    public static Dictionary<string, IniSetting> List = [];
    private static bool b_initialized = false;

    public static void Init()
    {
        if (b_initialized)
        {
            return;
        }

        Add(SettingKey.SoundPanningEnabled, IniSettingType.Bool, true);
        Add(SettingKey.SoundPanningStrength, IniSettingType.Float, 0.71f, 0f, 1f);
        Add(SettingKey.SoundPanningForceScreenSpace, IniSettingType.Bool, false);
        Add(SettingKey.SoundPanningInworldThreshold, IniSettingType.Int, 60, 0, 1000);
        Add(SettingKey.SoundPanningInworldDistance, IniSettingType.Int, 400, 0, 1000);
        Add(SettingKey.SoundAttenuationEnabled, IniSettingType.Bool, true);
        Add(SettingKey.SoundAttenuationMin, IniSettingType.Float, 0.6f, 0f, 1f);
        Add(SettingKey.SoundAttenuationForceScreenSpace, IniSettingType.Bool, false);
        Add(SettingKey.SoundAttenuationInworldThreshold, IniSettingType.Int, 60, 0, 1000);
        Add(SettingKey.SoundAttenuationInworldDistance, IniSettingType.Int, 500, 0, 1000);
        Add(SettingKey.LowHealthSaturationFactor, IniSettingType.Float, 0.71f, 0f, 1f, true);
        Add(SettingKey.LowHealthThreshold, IniSettingType.Float, 0.25f, 0f, 1f, true);
        Add(SettingKey.HideFilmgrain, IniSettingType.Bool, false);

        b_initialized = true;
    }
    
    public static void ResetToDefaults()
    {
        foreach (IniSetting setting in List.Values)
        {
            setting.Reset();
        }
    }

    private static void Add(string key, IniSettingType type, object value, object minValue = null, object maxValue = null, bool requiresRestart = false)
    {
        List.Add(key, new IniSetting(key, value, type, minValue, maxValue, requiresRestart));
    }
    private static void Add(SettingKey key, IniSettingType type, object value, object minValue = null, object maxValue = null, bool requiresRestart = false)
    {
        string keyString = GetKey(key);
        Add(keyString, type, value, minValue, maxValue, requiresRestart);
    }

    public static string GetKey(SettingKey key)
    {
        switch (key)
        {
            default:
                string mess = $"CONFIG.INI: Fail at GetKey, SettingKey '{key}' does not have a key!";
                Logger.LogError(mess);
                throw new Exception(mess);
            case SettingKey.SoundPanningEnabled: return "SOUNDPANNING_ENABLED";
            case SettingKey.SoundPanningStrength: return "SOUNDPANNING_STRENGTH";
            case SettingKey.SoundPanningForceScreenSpace: return "SOUNDPANNING_FORCE_SCREEN_SPACE";
            case SettingKey.SoundPanningInworldThreshold: return "SOUNDPANNING_INWORLD_THRESHOLD";
            case SettingKey.SoundPanningInworldDistance: return "SOUNDPANNING_INWORLD_DISTANCE";
            case SettingKey.SoundAttenuationEnabled: return "SOUNDATTENUATION_ENABLED";
            case SettingKey.SoundAttenuationMin: return "SOUNDATTENUATION_MIN";
            case SettingKey.SoundAttenuationForceScreenSpace: return "SOUNDATTENUATION_FORCE_SCREEN_SPACE";
            case SettingKey.SoundAttenuationInworldThreshold: return "SOUNDATTENUATION_INWORLD_THRESHOLD";
            case SettingKey.SoundAttenuationInworldDistance: return "SOUNDATTENUATION_INWORLD_DISTANCE"; ;
            case SettingKey.LowHealthSaturationFactor: return "LOW_HEALTH_SATURATION_FACTOR";
            case SettingKey.LowHealthThreshold: return "LOW_HEALTH_THRESHOLD";
            case SettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
        }
    }

    public static T Get<T>(SettingKey key, bool getDefault = false)
    {
        return Get<T>(GetKey(key), getDefault);
    }

    public static T Get<T>(string key, bool getDefault = false)
    {
        key = key.ToUpperInvariant();

        if (!List.ContainsKey(key))
        {
            string mess = $"CONFIG.INI: Trying to GET key '{key}', key not found!!";
            Logger.LogError(mess);
            throw new KeyNotFoundException(mess);
        }

        IniSetting setting = List[key];
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

        if (!List.ContainsKey(key))
        {
            string mess = $"CONFIG.INI: Trying to SET key '{key}', key not found!!";
            Logger.LogError(mess);
            throw new KeyNotFoundException(mess);
        }

        IniSetting setting = List[key];
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
}
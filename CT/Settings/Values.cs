using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SFDCT.Helper;

namespace SFDCT.Settings;

public static partial class Values
{
    public enum SettingKey
    {
        SoundPanningEnabled,
        SoundPanningStrength,
        SoundPanningForceScreenSpace,
        SoundPanningInworldThreshold,
        SoundPanningInworldDistance,
        SoundAttenuationEnabled,
        SoundAttenuationMin,
        SoundAttenuationForceScreenSpace,
        SoundAttenuationInworldThreshold,
        SoundAttenuationInworldDistance,
        LowHealthSaturationFactor,
        LowHealthThreshold,
        HideFilmgrain,
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
            case SettingKey.SoundAttenuationInworldDistance: return "SOUNDATTENUATION_INWORLD_DISTANCE";;
            case SettingKey.LowHealthSaturationFactor: return "LOW_HEALTH_SATURATION_FACTOR";
            case SettingKey.LowHealthThreshold: return "LOW_HEALTH_THRESHOLD";
            case SettingKey.HideFilmgrain: return "HIDE_FILMGRAIN";
        }
    }

    public static Dictionary<string, IniSetting> List = [];
    private static bool b_initialized = false;
    public static void Init()
    {
        if (b_initialized)
        {
            return;
        }

        Add(GetKey(SettingKey.SoundPanningEnabled), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundPanningStrength), 0.7f, 0f, 1f, IniSettingType.Float);
        Add(GetKey(SettingKey.SoundPanningForceScreenSpace), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundPanningInworldThreshold), 60, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundPanningInworldDistance), 400, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundAttenuationEnabled), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundAttenuationMin), 0.6f, 0f, 1f, IniSettingType.Float);
        Add(GetKey(SettingKey.SoundAttenuationForceScreenSpace), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundAttenuationInworldThreshold), 60, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundAttenuationInworldDistance), 500, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.LowHealthSaturationFactor), 0.7f, 0f, 1f, IniSettingType.Float, true);
        Add(GetKey(SettingKey.LowHealthThreshold), 0.25f, 0f, 1f, IniSettingType.Float, true);
        Add(GetKey(SettingKey.HideFilmgrain), false, IniSettingType.Bool);

        b_initialized = true;
    }
    
    public static void ApplyOverrides()
    {
        Logger.LogDebug("CONFIG.INI: Applied values to SFD's internals");
    }

    public static void ResetToDefaults()
    {
        foreach (IniSetting setting in List.Values)
        {
            setting.Reset();
        }
    }

    private static void Add(string key, object value, IniSettingType type, bool requiresRestart = false)
    {
        List.Add(key.ToUpperInvariant(), new IniSetting(key.ToUpperInvariant(), value, type, null, null, requiresRestart));
    }
    private static void Add(string key, object value, object minValue, object maxValue, IniSettingType type, bool requiresRestart = false)
    {
        List.Add(key.ToUpperInvariant(), new IniSetting(key.ToUpperInvariant(), value, type, minValue, maxValue, requiresRestart));
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

    public static T GetLimit<T>(string key, bool getMaxValue = false)
    {
        key = key.ToUpperInvariant();

        if (!List.ContainsKey(key))
        {
            string mess = $"CONFIG.INI: Trying to GET LIMIT key '{key}', key not found!!";
            Logger.LogError(mess);
            throw new KeyNotFoundException(mess);
        }

        IniSetting setting = List[key];
        if (setting.ValueType != typeof(T))
        {
            string mess = $"CONFIG.INI: Trying to GET LIMIT value '{key}' from type '{typeof(T)}', key is type {setting.ValueType}!!";
            Logger.LogError(mess);
            throw new Exception(mess);
        }

        return (T)setting.GetLimit(getMaxValue);
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

        if (setting.RequiresGameRestart && !Config.FirstRefresh)
        {
            string mess = $"CONFIG.INI: '{key}' requires GAME-RESTART to properly change from {setting.Value} to {value}!";
            Logger.LogWarn(mess);
        }

        if (!setting.Value.Equals(value))
        {
            Config.NeedsSaving = true;
        }

        setting.Value = value;
    }
}
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

        Add(GetKey(SettingKey.SoundPanningEnabled), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundPanningStrength), 0.71f, 0f, 1f, IniSettingType.Float);
        Add(GetKey(SettingKey.SoundPanningForceScreenSpace), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundPanningInworldThreshold), 60, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundPanningInworldDistance), 400, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundAttenuationEnabled), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundAttenuationMin), 0.6f, 0f, 1f, IniSettingType.Float);
        Add(GetKey(SettingKey.SoundAttenuationForceScreenSpace), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.SoundAttenuationInworldThreshold), 60, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.SoundAttenuationInworldDistance), 500, 0, 1000, IniSettingType.Int);
        Add(GetKey(SettingKey.LowHealthSaturationFactor), 0.71f, 0f, 1f, IniSettingType.Float, true);
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

        if (setting.RequiresGameRestart && !IniFile.FirstRefresh)
        {
            string mess = $"CONFIG.INI: '{key}' requires GAME-RESTART to properly change from {setting.Value} to {value}!";
            Logger.LogWarn(mess);
        }

        if (!setting.Value.Equals(value))
        {
            IniFile.NeedsSaving = true;
        }

        setting.Value = value;
    }
}
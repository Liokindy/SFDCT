using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFDCT.Bootstrap.Assets;
using SFDCT.Helper;

namespace SFDCT.Settings;

public static class Values
{
    public enum SettingKey
    {
        MenuColor,
        PlayerBlinkColor,
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
        MainMenuBgUseBlack,
        MainMenuTrackRandom,
        LowHealthSaturationFactor,
        LowHealthThreshold,
        Use140Assets,
        UseSfrColorsForTeam5Team6,
        AllowSpectators,
        AllowSpectatorsOnlyModerators,
        AllowSpectatorsCount,
        ExtendedProfile,
        HideFilmgrain,
    }
    public static string GetKey(SettingKey key)
    {
        switch(key)
        {
            default:
                string mess = $"CONFIG.INI: Fail at GetKey, SettingKey '{key}' does not have a key!";
                Logger.LogError(mess);
                throw new Exception(mess);
            case SettingKey.MenuColor:                          return "MENU_COLOR";
            case SettingKey.PlayerBlinkColor:                   return "PLAYER_BLINK_COLOR";
            case SettingKey.SoundPanningEnabled:                return "SOUNDPANNING_ENABLED";
            case SettingKey.SoundPanningStrength:               return "SOUNDPANNING_STRENGTH";
            case SettingKey.SoundPanningForceScreenSpace:       return "SOUNDPANNING_FORCE_SCREEN_SPACE";
            case SettingKey.SoundPanningInworldThreshold:       return "SOUNDPANNING_INWORLD_THRESHOLD";
            case SettingKey.SoundPanningInworldDistance:        return "SOUNDPANNING_INWORLD_DISTANCE";
            case SettingKey.SoundAttenuationEnabled:            return "SOUNDATTENUATION_ENABLED";
            case SettingKey.SoundAttenuationMin:                return "SOUNDATTENUATION_MIN";
            case SettingKey.SoundAttenuationForceScreenSpace:   return "SOUNDATTENUATION_FORCE_SCREEN_SPACE";
            case SettingKey.SoundAttenuationInworldThreshold:   return "SOUNDATTENUATION_INWORLD_THRESHOLD";
            case SettingKey.SoundAttenuationInworldDistance:    return "SOUNDATTENUATION_INWORLD_DISTANCE";
            case SettingKey.MainMenuBgUseBlack:                 return "MAINMENU_BG_USE_BLACK";
            case SettingKey.MainMenuTrackRandom:                return "MAINMENU_TRACK_RANDOM";
            case SettingKey.LowHealthSaturationFactor:          return "LOW_HEALTH_SATURATION_FACTOR";
            case SettingKey.LowHealthThreshold:                 return "LOW_HEALTH_THRESHOLD";
            case SettingKey.Use140Assets:                       return "USE_1_4_0_ASSETS";
            case SettingKey.UseSfrColorsForTeam5Team6:          return "USE_SFR_COLORS_FOR_TEAM5_TEAM6";
            case SettingKey.AllowSpectators:                    return "ALLOW_SPECTATORS";
            case SettingKey.AllowSpectatorsOnlyModerators:      return "ALLOW_SPECTATORS_ONLY_MODERATORS";
            case SettingKey.AllowSpectatorsCount:               return "ALLOW_SPECTATORS_COUNT";
            case SettingKey.ExtendedProfile:                    return "EXTENDEDPROFILES_PROFILE_";
            case SettingKey.HideFilmgrain:                      return "HIDE_FILMGRAIN";
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

        Add(GetKey(SettingKey.MenuColor), new Color(32, 0, 192), IniSettingType.Color);
        Add(GetKey(SettingKey.PlayerBlinkColor), new Color(255, 255, 255), IniSettingType.Color);
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
        for (int i = 1; i <= 8; i++)
        {
            Add(GetKey(SettingKey.ExtendedProfile) + i, 0, IniSettingType.Int);
        }
        Add(GetKey(SettingKey.MainMenuBgUseBlack), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.MainMenuTrackRandom), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.LowHealthSaturationFactor), 0.7f, 0f, 1f, IniSettingType.Float, true);
        Add(GetKey(SettingKey.LowHealthThreshold), 0.25f, 0f, 1f, IniSettingType.Float, true);
        Add(GetKey(SettingKey.Use140Assets), false, IniSettingType.Bool, true);
        Add(GetKey(SettingKey.UseSfrColorsForTeam5Team6), true, IniSettingType.Bool, false);

        Add(GetKey(SettingKey.AllowSpectators), false, IniSettingType.Bool);
        Add(GetKey(SettingKey.AllowSpectatorsOnlyModerators), true, IniSettingType.Bool);
        Add(GetKey(SettingKey.AllowSpectatorsCount), 4, 1, 4, IniSettingType.Int);
        Add(GetKey(SettingKey.HideFilmgrain), false, IniSettingType.Bool);

        b_initialized = true;
    }
    public static void ApplyOverrides()
    {
        SFD.Constants.COLORS.MENU_BLUE = Values.Get<Color>(GetKey(SettingKey.MenuColor));
        SFD.Constants.COLORS.PLAYER_FLASH_LIGHT = Values.Get<Color>(GetKey(SettingKey.PlayerBlinkColor));
        SFD.Constants.PLAYER_1_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "1");
        SFD.Constants.PLAYER_2_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "2");
        SFD.Constants.PLAYER_3_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "3");
        SFD.Constants.PLAYER_4_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "4");
        SFD.Constants.PLAYER_5_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "5");
        SFD.Constants.PLAYER_6_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "6");
        SFD.Constants.PLAYER_7_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "7");
        SFD.Constants.PLAYER_8_PROFILE = Values.Get<int>(GetKey(SettingKey.ExtendedProfile) + "8");

        Logger.LogDebug("CONFIG.INI: Applied values to SFD's internals");
    }
    public static void CheckOverrides()
    {
        for(int i = 1; i <= 8; i++)
        {
            int profVal = 0;
            switch (i)
            {
                case 1: profVal = SFD.Constants.PLAYER_1_PROFILE; break;
                case 2: profVal = SFD.Constants.PLAYER_2_PROFILE; break;
                case 3: profVal = SFD.Constants.PLAYER_3_PROFILE; break;
                case 4: profVal = SFD.Constants.PLAYER_4_PROFILE; break;
                case 5: profVal = SFD.Constants.PLAYER_5_PROFILE; break;
                case 6: profVal = SFD.Constants.PLAYER_6_PROFILE; break;
                case 7: profVal = SFD.Constants.PLAYER_7_PROFILE; break;
                case 8: profVal = SFD.Constants.PLAYER_8_PROFILE; break;
            }
        }
    }
    public static void ResetToDefaults()
    {
        foreach(IniSetting setting in List.Values)
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

    public enum IniSettingType
    {
        Float,
        Int,
        String,
        Color,
        Bool,
    }
    public class IniSetting
    {
        public IniSettingType Type = IniSettingType.Int;
        public Type ValueType
        {
            get
            {
                switch(Type)
                {
                    default:
                        return typeof(object);
                    case IniSettingType.Float:
                        return typeof(float);
                    case IniSettingType.Int:
                        return typeof(int);
                    case IniSettingType.String:
                        return typeof(string);
                    case IniSettingType.Color:
                        return typeof(Color);
                    case IniSettingType.Bool:
                        return typeof(bool);
                }
            }
        }
        
        public string Key;
        public object Value;
        public object MinValue;
        public object MaxValue;
        public readonly object Default;
        public readonly bool RequiresGameRestart;

        public IniSetting(string saveKey, object saveValue, IniSettingType saveType, object minValue = null, object maxValue = null, bool requiresRestart = false)
        {
            this.Key = saveKey.ToUpperInvariant();
            this.Value = saveValue;
            this.MaxValue = maxValue;
            this.MinValue = minValue;
            this.Default = this.Value;
            this.Type = saveType;
            this.RequiresGameRestart = requiresRestart;
        }
        public void Save(IniHandler Handler, bool saveAsDefault = false)
        {
            string line = this.Key + "=" + (saveAsDefault ? this.Default.ToString() : this.Value.ToString());
            switch (this.Type)
            {
                case IniSettingType.Color:
                    // Use SFD's way to store color
                    string colorString = Constants.ColorToString((Color)(saveAsDefault ? this.Default : this.Value));
                    line = this.Key + "=" + colorString;
                    break;
                case IniSettingType.Float:
                    string floatString = ((float)(saveAsDefault ? this.Default : this.Value)).ToString(CultureInfo.InvariantCulture);
                    line = this.Key + "=" + floatString.Replace(',', '.');
                    break;
            }
            Handler.ReadLine(line);

            Logger.LogDebug($"CONFIG.INI: Saved '{this.Key}' {this.Type}: {this.Value}, to '{line}'");
        }
        public void Load(IniHandler Handler)
        {
            if (Handler.TryReadValue(this.Key, out string temp))
            {
                object NewValue = this.Value;
                switch (this.Type)
                {
                    case IniSettingType.Float:
                        NewValue = float.Parse(temp.Replace(',', '.'), CultureInfo.InvariantCulture);
                        if (this.MaxValue != null && (float)NewValue > (float)this.MaxValue)
                        {
                            NewValue = (float)this.MaxValue;
                        }
                        if (this.MinValue != null && (float)NewValue < (float)this.MinValue)
                        {
                            NewValue = (float)this.MinValue;
                        }
                        break;
                    case IniSettingType.Int:
                        NewValue = int.Parse(temp);
                        if (this.MaxValue != null && (int)NewValue > (int)this.MaxValue)
                        {
                            NewValue = (int)this.MaxValue;
                        }
                        if (this.MinValue != null && (int)NewValue < (int)this.MinValue)
                        {
                            NewValue = (int)this.MinValue;
                        }
                        break;
                    case IniSettingType.String:
                        NewValue = temp;
                        break;
                    case IniSettingType.Color:
                        Color tempColor;
                        Handler.TryReadValueColor(this.Key, (Color)this.Default, out tempColor);
                        NewValue = tempColor;
                        break;
                    case IniSettingType.Bool:
                        NewValue = bool.Parse(temp);
                        break;
                }

                if (!Config.FirstRefresh && RequiresGameRestart && !NewValue.Equals(this.Value))
                {
                    Logger.LogWarn($"CONFIG.INI: Failed to load '{this.Key}' from {this.Value} to {NewValue}, requires a game-restart!");
                }
                else
                {
                    this.Value = NewValue;
                    Logger.LogDebug($"CONFIG.INI: Loaded '{this.Key}' {this.Type}: {this.Value}, from '{temp}'");
                }
            }
            else
            {
                Logger.LogWarn($"CONFIG.INI: '{this.Key}', {this.Type} was not found in the current ini, adding...");
                this.Save(Handler, true);
            }
        }
        public object Get(bool getDefault = false)
        {
            switch (this.Type)
            {
                case IniSettingType.Float:
                    return (float)(getDefault ? this.Default : this.Value);
                case IniSettingType.Int:
                    return (int)(getDefault ? this.Default : this.Value);
                case IniSettingType.String:
                    return (string)(getDefault ? this.Default : this.Value);
                case IniSettingType.Color:
                    return (Color)(getDefault ? this.Default : this.Value);
                case IniSettingType.Bool:
                    return (bool)(getDefault ? this.Default : this.Value);
                default:
                    return (getDefault ? this.Default : this.Value);
            }
        }
        public void Reset()
        {
            if (!Config.FirstRefresh && RequiresGameRestart && !this.Default.Equals(this.Value))
            {
                Logger.LogWarn($"CONFIG.INI: Failed to RESET '{this.Key}' from {this.Value} to {this.Default}, requires a game-restart!");
                return;
            }

            switch (this.Type)
            {
                case IniSettingType.Float:
                    this.Value = (float)(this.Default);
                    break;
                case IniSettingType.Int:
                    this.Value = (int)(this.Default);
                    break;
                case IniSettingType.String:
                    this.Value = (string)(this.Default);
                    break;
                case IniSettingType.Color:
                    this.Value = (Color)(this.Default);
                    break;
                case IniSettingType.Bool:
                    this.Value = (bool)(this.Default);
                    break;
                default:
                    this.Value = this.Default;
                    break;
            }
        }
        public object GetLimit(bool getMaxValue)
        {
            switch (this.Type)
            {
                case IniSettingType.Float:
                    return (float)(getMaxValue ? this.MaxValue : this.MinValue);
                case IniSettingType.Int:
                    return (int)(getMaxValue ? this.MaxValue : this.MinValue);
                case IniSettingType.String:
                    return (string)(getMaxValue ? this.MaxValue : this.MinValue);
                case IniSettingType.Color:
                    return (Color)(getMaxValue ? this.MaxValue : this.MinValue);
                case IniSettingType.Bool:
                    return (bool)(getMaxValue ? this.MaxValue : this.MinValue);
                default:
                    return (getMaxValue ? this.MaxValue : this.MinValue);
            }
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using SFD.Code;
using SFDCT.Helper;

namespace SFDCT.Settings;

public static class Values
{
    public static Dictionary<string, IniSetting> List = [];
    private static bool b_initialized = false;
    public static void Init()
    {
        if (b_initialized)
        {
            return;
        }

        Add("MENU_COLOR", new Color(32, 0, 192), IniSettingType.Color);
        Add("PLAYER_BLINK_COLOR", new Color(255, 255, 255), IniSettingType.Color);
        Add("SOUNDPANNING_ENABLED", true, IniSettingType.Bool);
        Add("SOUNDPANNING_STRENGTH", 0.7f, 0f, 1f, IniSettingType.Float);
        Add("SOUNDPANNING_FORCE_SCREEN_SPACE", false, IniSettingType.Bool);
        Add("SOUNDPANNING_INWORLD_THRESHOLD", 60f, 0f, null, IniSettingType.Float);
        Add("SOUNDPANNING_INWORLD_DISTANCE", 400f, 0f, null, IniSettingType.Float);
        Add("SOUNDATTENUATION_ENABLED", true, IniSettingType.Bool);
        Add("SOUNDATTENUATION_MIN", 0.6f, 0f, 1f, IniSettingType.Float);
        Add("SOUNDATTENUATION_FORCE_SCREEN_SPACE", false, IniSettingType.Bool);
        Add("SOUNDATTENUATION_INWORLD_THRESHOLD", 60f, 0f, null, IniSettingType.Float);
        Add("SOUNDATTENUATION_INWORLD_DISTANCE", 500f, 0f, null, IniSettingType.Float);
        for (int i = 1; i <= 8; i++)
        {
            Add($"EXTENDEDPROFILES_{i}_PROFILE", 0, IniSettingType.Int);
        }
        Add("MAINMENU_BG_USE_BLACK", true, IniSettingType.Bool);
        Add("MAINMENU_TRACK_RANDOM", false, IniSettingType.Bool);
        Add("LOW_HEALTH_SATURATION_FACTOR", 0.7f, 0f, 1f, IniSettingType.Float, true);
        Add("LOW_HEALTH_THRESHOLD", 0.25f, 0f, 1f, IniSettingType.Float, true);
        Add("USE_1_4_0_ASSETS", false, IniSettingType.Bool, true);
        Add("USE_SFR_COLORS_FOR_TEAM5_TEAM6", true, IniSettingType.Bool, false);

        Add("ALLOW_SPECTATORS", false, IniSettingType.Bool);
        Add("ALLOW_SPECTATORS_ONLY_MODERATORS", true, IniSettingType.Bool);
        Add("ALLOW_SPECTATORS_COUNT", 4, 1, 4, IniSettingType.Int);

        b_initialized = true;
    }
    public static void ApplyOverrides()
    {
        SFD.Constants.COLORS.MENU_BLUE = Values.GetColor("MENU_COLOR");
        SFD.Constants.COLORS.PLAYER_FLASH_LIGHT = Values.GetColor("PLAYER_BLINK_COLOR");
        SFD.Constants.PLAYER_1_PROFILE = Values.GetInt("EXTENDEDPROFILES_1_PROFILE");
        SFD.Constants.PLAYER_2_PROFILE = Values.GetInt("EXTENDEDPROFILES_2_PROFILE");
        SFD.Constants.PLAYER_3_PROFILE = Values.GetInt("EXTENDEDPROFILES_3_PROFILE");
        SFD.Constants.PLAYER_4_PROFILE = Values.GetInt("EXTENDEDPROFILES_4_PROFILE");
        SFD.Constants.PLAYER_5_PROFILE = Values.GetInt("EXTENDEDPROFILES_5_PROFILE");
        SFD.Constants.PLAYER_6_PROFILE = Values.GetInt("EXTENDEDPROFILES_6_PROFILE");
        SFD.Constants.PLAYER_7_PROFILE = Values.GetInt("EXTENDEDPROFILES_7_PROFILE");
        SFD.Constants.PLAYER_8_PROFILE = Values.GetInt("EXTENDEDPROFILES_8_PROFILE");

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
    private static void Add(string key, object value, IniSettingType type, bool requiresRestart = false)
    {
        List.Add(key.ToUpperInvariant(), new IniSetting(key.ToUpperInvariant(), value, type, null, null, requiresRestart));
    }
    private static void Add(string key, object value, object minValue, object maxValue, IniSettingType type, bool requiresRestart = false)
    {
        List.Add(key.ToUpperInvariant(), new IniSetting(key.ToUpperInvariant(), value, type, minValue, maxValue, requiresRestart));
    }
    public static bool SetSetting(string key, object newValue)
    {
        if (List.ContainsKey(key.ToUpperInvariant()))
        {
            List[key.ToUpperInvariant()].Value = newValue;
            Config.NeedsSaving = true;
            return true;
        }
        return false;
    }
    public static float GetFloat(string key)
    {
        return (float)List[key.ToUpperInvariant()].Get();
    }
    public static int GetInt(string key)
    {
        return (int)List[key.ToUpperInvariant()].Get();
    }
    public static string GetString(string key)
    {
        return (string)List[key.ToUpperInvariant()].Get();
    }
    public static Color GetColor(string key)
    {
        return (Color)List[key.ToUpperInvariant()].Get();
    }
    public static bool GetBool(string key)
    {
        return (bool)List[key.ToUpperInvariant()].Get();
    }

    // Settings base class
    public enum IniSettingType
    {
        Float = 0,
        Int = 1,
        String = 2,
        Color = 3,
        Bool = 4,
    }
    public class IniSetting
    {
        public IniSettingType Type = IniSettingType.Int;
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
                    string colorString = SFD.Constants.ColorToString((Color)(saveAsDefault ? this.Default : this.Value));
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
        public object Get()
        {
            switch (this.Type)
            {
                case IniSettingType.Float:
                    return (float)this.Value;
                case IniSettingType.Int:
                    return (int)this.Value;
                case IniSettingType.String:
                    return (string)this.Value;
                case IniSettingType.Color:
                    return (Color)this.Value;
                case IniSettingType.Bool:
                    return (bool)this.Value;
                default:
                    return this.Value;
            }
        }
    }
}
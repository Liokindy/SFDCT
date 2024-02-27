using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SFD.Code;
using SFDCT.Helper;
using SFDCT.Misc;

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
        Add("LAZER_USE_REAL_ACCURACY", true, IniSettingType.Bool);
        Add("SOUNDPANNING_ENABLED", true, IniSettingType.Bool);
        Add("SOUNDPANNING_STRENGTH", 0.8f, IniSettingType.Float);
        Add("SOUNDPANNING_FORCE_SCREEN_SPACE", false, IniSettingType.Bool);
        Add("SOUNDPANNING_INWORLD_THRESHOLD", 80f, IniSettingType.Float);
        Add("SOUNDPANNING_INWORLD_DISTANCE", 1000f, IniSettingType.Float);

        for(int i = 1; i <= 8; i++)
        {
            Add($"EXTENDEDPROFILES_{i}_PROFILE", 0, IniSettingType.Int);
        }

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

            Values.SetSetting($"EXTENDEDPROFILES_{i}_PROFILE", profVal);
        }
    }
    private static void Add(string key, object value, IniSettingType type, bool experimental = false)
    {
        List.Add(key.ToUpper(), new IniSetting(key.ToUpper(), value, type, "", experimental));
    }
    public static bool SetSetting(string key, object newValue)
    {
        if (List.ContainsKey(key.ToUpper()))
        {
            List[key.ToUpper()].Value = newValue;
            ConfigIni.NeedsSaving = true;
            return true;
        }
        return false;
    }
    public static float GetFloat(string key)
    {
        return (float)List[key.ToUpper()].Get();
    }
    public static int GetInt(string key)
    {
        return (int)List[key.ToUpper()].Get();
    }
    public static string GetString(string key)
    {
        return (string)List[key.ToUpper()].Get();
    }
    public static Color GetColor(string key)
    {
        return (Color)List[key.ToUpper()].Get();
    }
    public static bool GetBool(string key)
    {
        return (bool)List[key.ToUpper()].Get();
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
        public readonly object Default;
        public string Description;
        public bool IsExperimental;
        public IniSetting(string saveKey, object saveValue, IniSettingType saveType, string valueDesc = "", bool experimental = false)
        {
            this.Key = saveKey.ToUpper();
            this.Value = saveValue;
            this.Default = this.Value;
            this.Type = saveType;
            this.Description = valueDesc;
            this.IsExperimental = experimental;
        }
        public void Save(IniHandler Handler, bool saveAsDefault = false)
        {
            if (this.IsExperimental) { return; }

            if (!string.IsNullOrEmpty(this.Description))
            {
                Handler.ReadLine(";" + this.Description);
            }

            switch (this.Type)
            {
                default:
                    Handler.ReadLine(this.Key + "=" + (saveAsDefault ? this.Default.ToString() : this.Value.ToString()));
                    break;
                case IniSettingType.Color:
                    // Use SFD's way to store color
                    string colorString = SFD.Constants.ColorToString((Color)(saveAsDefault ? this.Default : this.Value));
                    Handler.ReadLine(this.Key + "=" + colorString);
                    break;
            }

            Logger.LogDebug($"CONFIG.INI: Saved '{this.Key}', {this.Type}: {this.Value}");
        }
        public void Load(IniHandler Handler)
        {
            if (Handler.TryReadValue(this.Key, out string temp))
            {
                switch (this.Type)
                {
                    case IniSettingType.Float:
                        this.Value = float.Parse(temp);
                        break;
                    case IniSettingType.Int:
                        this.Value = int.Parse(temp);
                        break;
                    case IniSettingType.String:
                        this.Value = temp;
                        break;
                    case IniSettingType.Color:
                        Color tempColor;
                        Handler.TryReadValueColor(this.Key, Color.Magenta, out tempColor);
                        this.Value = tempColor;
                        break;
                    case IniSettingType.Bool:
                        this.Value = bool.Parse(temp);
                        break;
                }

                if (this.IsExperimental) { return; }

                Logger.LogDebug($"CONFIG.INI: Loaded '{this.Key}', {this.Type}: {this.Value}");
            }
            else
            {
                if (this.IsExperimental) { return; }

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
                    return null;
            }
        }
    }
}
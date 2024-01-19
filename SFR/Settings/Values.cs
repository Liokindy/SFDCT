using System;
using System.Collections.Generic;
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
    public static Dictionary<string, IniSetting> List = new();
    private static bool b_initialized = false;
    public static void Init()
    {
        if (b_initialized)
        {
            Logger.LogWarn("CONFIG.INI: Tried to initialize settings list while it is already initialized");
            return;
        }

        // Here you can add more settings to be written in the config.ini,
        // and read them later in code.
        Add("Security settings", "USE_OBFUSCATED_HOST_ACCOUNT_NAME", false, IniSettingType.Bool);
        Add("OBFUSCATED_HOST_ACCOUNT_NAME", "Unnamed", IniSettingType.String);
        Add("VOTE_KICKING_ENABLED", true, IniSettingType.Bool);
        Add("VOTE_KICKING_COOLDOWN_MINUTES", 3, IniSettingType.Int);
        Add("VOTE_KICKING_DURATION_SECONDS", 35, IniSettingType.Int);

        Add("Misc customization settings", "MENU_COLOR", new Color(32, 0, 192), IniSettingType.Color);
        Add("PLAYER_BLINK_COLOR", new Color(255, 255, 255), IniSettingType.Color);
        Add("LAZER_USE_REAL_ACCURACY", true, IniSettingType.Bool);

        Add("Sound panning settings", "SOUNDPANNING_ENABLED", true, IniSettingType.Bool);
        Add("SOUNDPANNING_STRENGTH", 1f, IniSettingType.Float);
        Add("SOUNDPANNING_SCREEN_SPACE", false, IniSettingType.Bool);
        Add("SOUNDPANNING_INWORLD_THRESHOLD", 64f, IniSettingType.Float);
        Add("SOUNDPANNING_INWORLD_DISTANCE", 360f, IniSettingType.Float);
        b_initialized = true;
    }
    public static void ApplyOverrides()
    {
        Logger.LogDebug("CONFIG.INI: Overriding SFD values...");

        SFD.Constants.COLORS.MENU_BLUE = Values.GetColor("MENU_COLOR");
        SFD.Constants.COLORS.PLAYER_FLASH_LIGHT = Values.GetColor("PLAYER_BLINK_COLOR");

        SFD.Constants.PREFFERED_GAMEWORLD_SIMULATION_CHUNK_SIZE = 5f; // 30f;
        SFD.Constants.PREFFERED_GAMEWORLD_SIMULATION_UPDATE_FPS = 11f; // 66f;

        Logger.LogDebug("CONFIG.INI: SFD Values overwritten");
    }
    private static void Add(string valueDesc, string key, object value, IniSettingType type)
    {
        List.Add(key.ToUpper(), new IniSetting(key.ToUpper(), value, type, valueDesc));
    }
    private static void Add(string key, object value, IniSettingType type)
    {
        Values.Add("", key, value, type);
    }
    public static bool SetSetting(string key, object newValue)
    {
        if (List.ContainsKey(key.ToUpper()))
        {
            List[key.ToUpper()].Value = newValue;
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
        public IniSetting(string saveKey, object saveValue, IniSettingType saveType, string valueDesc = "")
        {
            this.Key = saveKey.ToUpper();
            this.Value = saveValue;
            this.Default = this.Value;
            this.Type = saveType;
            this.Description = valueDesc;
        }
        public void Save(IniHandler Handler, bool saveAsDefault = false)
        {
            Logger.LogDebug($"CONFIG.INI: Saving value... '{this.Key}', Type: {this.Type}");
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
        }
        public void Load(IniHandler Handler)
        {
            string temp;
            Logger.LogDebug($"CONFIG.INI: Reading value... '{this.Key}', Type: {this.Type}");
            if (Handler.TryReadValue(this.Key, out temp))
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
                Logger.LogDebug($"CONFIG.INI: '{this.Key}' found - {this.Value.ToString()}");
            }
            else
            {
                Logger.LogError($"CONFIG.INI: '{this.Key}' not found.");
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
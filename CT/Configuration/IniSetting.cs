using Microsoft.Xna.Framework;
using SFD;
using SFD.Code;
using SFDCT.Helper;
using System;
using System.Globalization;

namespace SFDCT.Configuration;

public class IniSetting
{
    public object Value
    {
        get { return this.m_currentValue; }
        set
        {
            if (value.GetType() == this.ValueType)
            {
                this.m_currentValue = value;
            }
        }
    }
    public object MinValue { get { return this.m_minValue; } }
    public object MaxValue { get { return this.m_maxValue; } }
    public object Default { get { return this.m_defaultValue; } }
    public bool RequiresGameRestart { get { return this.m_requiresGameRestart; } }
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(this.m_name))
            {
                return this.m_key;
            }
            return LanguageHelper.GetText("sfdct.setting.name." + this.m_name);
        }
    }
    public string Help
    {
        get
        {
            if (string.IsNullOrEmpty(this.m_help))
            {
                return this.m_type.ToString();
            }
            return LanguageHelper.GetText("sfdct.setting.help." + this.m_help);
        }
    }
    public string Category
    {
        get
        {
            if (string.IsNullOrEmpty(this.m_category))
            {
                return "Unknown";
            }
            return LanguageHelper.GetText("sfdct.setting.category." + this.m_category);
        }
    }
    public IniSettingType Type { get { return this.m_type; } }

    public Type ValueType
    {
        get
        {
            switch (m_type)
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

    private string m_key;
    private SettingKey m_settingKey;
    private IniSettingType m_type;
    private object m_currentValue;
    private readonly bool m_requiresGameRestart;
    private readonly object m_defaultValue;
    private object m_minValue;
    private object m_maxValue;
    private string m_name;
    private string m_help;
    private string m_category;

    public IniSetting(string saveKey, SettingKey settingKey, IniSettingType saveType, object saveValue, bool requiresRestart = false, object minValue = null, object maxValue = null, string nameString = "", string helpString = "")
    {
        m_key = saveKey.ToUpperInvariant();
        m_settingKey = settingKey;
        m_type = saveType;
        m_currentValue = saveValue;
        m_defaultValue = saveValue;
        m_minValue = minValue;
        m_maxValue = maxValue;
        m_requiresGameRestart = requiresRestart;
        m_name = nameString;
        m_help = helpString;
    }

    public IniSetting(string saveKey, IniSettingType saveType, object saveValue, bool requiresRestart = false, object minValue = null, object maxValue = null, string nameString = "", string helpString = "", string categoryString = "")
    {
        m_key = saveKey.ToUpperInvariant();
        m_settingKey = SettingKey.None;
        m_type = saveType;
        m_currentValue = saveValue;
        m_defaultValue = saveValue;
        m_minValue = minValue;
        m_maxValue = maxValue;
        m_requiresGameRestart = requiresRestart;
        m_name = nameString;
        m_help = helpString;
        m_category = categoryString;
    }

    public void Save(IniHandler Handler, bool saveAsDefault = false)
    {
        string line = m_key + "=" + (saveAsDefault ? m_defaultValue.ToString() : m_currentValue.ToString());

        switch (m_type)
        {
            case IniSettingType.Color:
                // Use SFD's way to store color
                string colorString = Constants.ColorToString((Color)(saveAsDefault ? m_defaultValue : m_currentValue));
                line = m_key + "=" + colorString;
                break;
            case IniSettingType.Float:
                string floatString = ((float)(saveAsDefault ? m_defaultValue : m_currentValue)).ToString(CultureInfo.InvariantCulture);
                line = m_key + "=" + floatString.Replace(',', '.');
                break;
        }

        Handler.ReadLine(line);
        Logger.LogDebug($"CONFIG.INI: Saved '{m_key}' {m_type}: {m_currentValue}, to '{line}'");
    }

    public void Load(IniHandler Handler)
    {
        if (Handler.TryReadValue(m_key, out string temp))
        {
            object NewValue = m_currentValue;
            switch (m_type)
            {
                case IniSettingType.Float:
                    NewValue = float.Parse(temp.Replace(',', '.'), CultureInfo.InvariantCulture);
                    if (m_maxValue != null && (float)NewValue > (float)m_maxValue)
                    {
                        NewValue = (float)m_maxValue;
                    }
                    if (m_minValue != null && (float)NewValue < (float)m_minValue)
                    {
                        NewValue = (float)m_minValue;
                    }
                    break;
                case IniSettingType.Int:
                    NewValue = int.Parse(temp);
                    if (m_maxValue != null && (int)NewValue > (int)m_maxValue)
                    {
                        NewValue = (int)m_maxValue;
                    }
                    if (m_minValue != null && (int)NewValue < (int)m_minValue)
                    {
                        NewValue = (int)m_minValue;
                    }
                    break;
                case IniSettingType.String:
                    NewValue = temp;
                    break;
                case IniSettingType.Color:
                    Color tempColor;
                    Handler.TryReadValueColor(m_key, (Color)m_defaultValue, out tempColor);
                    NewValue = tempColor;
                    break;
                case IniSettingType.Bool:
                    NewValue = bool.Parse(temp);
                    break;
            }

            if (!IniFile.FirstRefresh && m_requiresGameRestart && !NewValue.Equals(m_currentValue))
            {
                Logger.LogWarn($"CONFIG.INI: Failed to load '{m_key}' from {m_currentValue} to {NewValue}, requires a game-restart!");
            }
            else
            {
                m_currentValue = NewValue;
                Logger.LogDebug($"CONFIG.INI: Loaded '{m_key}' {m_type}: {m_currentValue}, from '{temp}'");
            }
        }
        else
        {
            Logger.LogWarn($"CONFIG.INI: '{m_key}', {m_type} was not found in the current ini, adding...");
            this.Save(Handler, true);
        }
    }
    public object Get(bool getDefault = false)
    {
        switch (m_type)
        {
            case IniSettingType.Float:
                return (float)(getDefault ? m_defaultValue : m_currentValue);
            case IniSettingType.Int:
                return (int)(getDefault ? m_defaultValue : m_currentValue);
            case IniSettingType.String:
                return (string)(getDefault ? m_defaultValue : m_currentValue);
            case IniSettingType.Color:
                return (Color)(getDefault ? m_defaultValue : m_currentValue);
            case IniSettingType.Bool:
                return (bool)(getDefault ? m_defaultValue : m_currentValue);
            default:
                return (getDefault ? m_defaultValue : m_currentValue);
        }
    }
    public void Reset()
    {
        if (!IniFile.FirstRefresh && m_requiresGameRestart && !m_defaultValue.Equals(m_currentValue))
        {
            Logger.LogWarn($"CONFIG.INI: Failed to RESET '{m_key}' from {m_currentValue} to {m_defaultValue}, requires a game-restart!");
            return;
        }

        switch (m_type)
        {
            case IniSettingType.Float:
                m_currentValue = (float)(m_defaultValue);
                break;
            case IniSettingType.Int:
                m_currentValue = (int)(m_defaultValue);
                break;
            case IniSettingType.String:
                m_currentValue = (string)(m_defaultValue);
                break;
            case IniSettingType.Color:
                m_currentValue = (Color)(m_defaultValue);
                break;
            case IniSettingType.Bool:
                m_currentValue = (bool)(m_defaultValue);
                break;
            default:
                m_currentValue = m_defaultValue;
                break;
        }
    }
    public object GetLimit(bool getMaxValue)
    {
        switch (m_type)
        {
            case IniSettingType.Float:
                return (float)(getMaxValue ? m_maxValue : m_minValue);
            case IniSettingType.Int:
                return (int)(getMaxValue ? m_maxValue : m_minValue);
            case IniSettingType.String:
                return (string)(getMaxValue ? m_maxValue : m_minValue);
            case IniSettingType.Color:
                return (Color)(getMaxValue ? m_maxValue : m_minValue);
            case IniSettingType.Bool:
                return (bool)(getMaxValue ? m_maxValue : m_minValue);
            default:
                return (getMaxValue ? m_maxValue : m_minValue);
        }
    }
}
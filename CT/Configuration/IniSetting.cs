using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Code;
using SFDCT.Helper;

namespace SFDCT.Configuration;

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
            switch (Type)
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

            if (!IniFile.FirstRefresh && RequiresGameRestart && !NewValue.Equals(this.Value))
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
        if (!IniFile.FirstRefresh && RequiresGameRestart && !this.Default.Equals(this.Value))
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
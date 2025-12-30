using SFD.Code;

namespace SFDCT.Configuration;

internal class CTIniHandler : IniHandler
{
    internal void ReadLine(string key, string value)
    {
        this.ReadLine(key + "=" + value);
    }

    internal void ReadLine(string key, bool value)
    {
        this.ReadLine(key + "=" + (value ? "1" : "0"));
    }

    internal void ReadLine(string key, int value)
    {
        this.ReadLine(key + "=" + value.ToString());
    }

    internal void ReadLine(string key, float value)
    {
        this.ReadLine(key + "=" + value.ToString());
    }

    internal void ReadLine(string key, Microsoft.Xna.Framework.Color value)
    {
        this.ReadLine(key + "=" + SFD.Constants.ColorToString(value));
    }

    internal void ReadLine(string key, SFD.GameKeyboard.GamePadCode value)
    {
        this.ReadLine(key + "=" + value.ToString());
    }

    internal string ReadValueString(string key, string backupValue)
    {
        string value = this.ReadValue(key);
        if (string.IsNullOrEmpty(value))
        {
            return backupValue;
        }

        return value;
    }
}

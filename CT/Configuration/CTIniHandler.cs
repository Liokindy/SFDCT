using SFD.Code;

namespace SFDCT.Configuration;

internal class CTIniHandler : IniHandler
{
    internal void ReadLine(string key, string value)
    {
        ReadLine(key + "=" + value);
    }

    internal void ReadLine(string key, bool value)
    {
        ReadLine(key + "=" + (value ? "1" : "0"));
    }

    internal void ReadLine(string key, int value)
    {
        ReadLine(key + "=" + value.ToString());
    }

    internal void ReadLine(string key, Microsoft.Xna.Framework.Color value)
    {
        ReadLine(key + "=" + SFD.Constants.ColorToString(value));
    }

    internal void ReadLine(string key, SFD.GameKeyboard.GamePadCode value)
    {
        ReadLine(key + "=" + value.ToString());
    }

    internal string ReadValueString(string key, string backupValue)
    {
        string value = ReadValue(key);
        if (string.IsNullOrEmpty(value))
        {
            return backupValue;
        }

        return value;
    }
}

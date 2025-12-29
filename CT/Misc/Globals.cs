using System.IO;

namespace SFDCT.Misc;

public static class Globals
{
    public readonly struct Paths
    {
        public readonly static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");
        public readonly static string ConfigurationIni = Path.Combine(SFDCT, "config.ini");
        public readonly static string Content = Path.Combine(SFDCT, "Content");
        public readonly static string SubContent = Path.Combine(SFDCT, "SubContent");
        public readonly static string Data = Path.Combine(Content, "Data");
        public readonly static string Language = Path.Combine(Data, "Misc", "Language");
    }

    public readonly struct Version
    {
        public const string SFDCT = "v.2.0.0";
        public const string SFD = "v.1.4.2";
    }
}
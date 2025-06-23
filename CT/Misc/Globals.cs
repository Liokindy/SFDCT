using System.IO;

namespace SFDCT.Misc;

public static class Globals
{
    public readonly struct Paths
    {
        public readonly static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");

        public static string ConfigurationIni { get { return Path.Combine(SFDCT, "config.ini"); } }
        public static string Content { get { return Path.Combine(SFDCT, "Content"); } }
        public static string Data { get { return Path.Combine(Content, "Data"); } }
        public static string Language { get { return Path.Combine(Data, "Misc", "Language"); } }
    }

    public struct Version
    {
        public static string SFDCT = "v.1.0.8";
        public static string SFD = "v.1.4.2";
    }
}
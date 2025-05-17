using System.IO;

namespace SFDCT.Misc;

public static class Globals
{
    public readonly struct Paths
    {
        public readonly static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");
        public readonly static string CONFIGURATIONINI = Path.Combine(SFDCT, "config.ini");
        public readonly static string CONTENT = Path.Combine(SFDCT, "Content");
        public readonly static string DATA = Path.Combine(CONTENT, "Data");
    }

    public struct Version
    {
        public static string SFDCT = "v.1.0.7_dev";
        public static string SFD = "v.1.4.1b";
    }
}
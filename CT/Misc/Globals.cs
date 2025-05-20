using System.IO;

namespace SFDCT.Misc;

public static class Globals
{
    public readonly struct Paths
    {
        public readonly static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");

        public static string CONFIGURATIONINI { get { return Path.Combine(SFDCT, "config.ini"); } }
        public static string CONTENT { get { return Path.Combine(SFDCT, "Content"); } }
        public static string DATA { get { return Path.Combine(CONTENT, "Data"); } }
    }

    public struct Version
    {
        public static string SFDCT = "v.1.0.7";
        public static string SFD = "v.1.4.1b";
    }
}
using System.IO;

namespace SFDCT.Misc;

internal static class Globals
{
    internal readonly struct Paths
    {
        internal static string SFDCT { get { return Path.Combine(Program.GameDirectory, "SFDCT"); } }
        internal static string ConfigurationIni { get { return Path.Combine(SFDCT, "config.ini"); } }
        internal static string Content { get { return Path.Combine(SFDCT, "Content"); } }
        internal static string SubContent { get { return Path.Combine(SFDCT, "SubContent"); } }
        internal static string Data { get { return Path.Combine(Content, "Data"); } }
        internal static string Language { get { return Path.Combine(Data, "Misc", "Language"); } }
        internal static string Commands { get { return Path.Combine(Data, "Misc", "Commands"); } }
    }

    internal readonly struct Version
    {
        internal const string SFDCT = "v.2.16.0";
        internal const string SFD = "v.1.5.0";
    }
}
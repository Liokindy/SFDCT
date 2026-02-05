using System.IO;

namespace SFDCT.Misc;

internal static class Globals
{
    internal readonly struct Paths
    {
        internal readonly static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");
        internal readonly static string ConfigurationIni = Path.Combine(SFDCT, "config.ini");
        internal readonly static string Content = Path.Combine(SFDCT, "Content");
        internal readonly static string SubContent = Path.Combine(SFDCT, "SubContent");
        internal readonly static string Data = Path.Combine(Content, "Data");
        internal readonly static string Language = Path.Combine(Data, "Misc", "Language");
        internal readonly static string Commands = Path.Combine(Data, "Misc", "Commands");
    }

    internal readonly struct Version
    {
        internal const string SFDCT = "v.2.6.1";
        internal const string SFD = "v.1.4.2";
    }
}
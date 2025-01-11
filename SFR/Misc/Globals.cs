using SFD;
using System.IO;

namespace SFDCT.Misc;

public static class Globals
{
    public readonly struct Paths
    {
        public static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");

        public static string CONTENT = Path.Combine(SFDCT, "Content");
        public static string CONFIGURATIONINI = Path.Combine(SFDCT, "config.ini");
        public static string PROFILES = Path.Combine(CONTENT, "Profile");
    }
    public readonly struct Version
    {
        public static string SFD = "v.1.3.7d";
        public static string SFDCT = "v.1.0.6_dev";
        public static bool INDEV
        {
            get
            {
                return SFDCT.EndsWith("dev");
            }
        }
        public static string LABEL
        {
            get
            {
                return SFDCT;
            }
        }
    }

    public static int SLOTCOUNT
    {
        get
        {
            if (HOST_GAME_EXTENDED_SLOTS)
            {
                return HOST_GAME_SLOT_COUNT;
            }
            return 8;
        }
    }
    public static bool HOST_GAME_EXTENDED_SLOTS = false;
    public static int HOST_GAME_SLOT_COUNT = 8;
    public static byte[] HOST_GAME_SLOT_STATES = new byte[HOST_GAME_SLOT_COUNT];
    public static Team[] HOST_GAME_SLOT_TEAMS = new Team[HOST_GAME_SLOT_COUNT];
}
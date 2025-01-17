using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
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

        public readonly static string PROFILES = Path.Combine(DATA, "Profile");
        public readonly static string SCRIPTS = Path.Combine(DATA, "Scripts");
    }
    public struct Version
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
    public struct Colors
    {
        public static Color TEAM_SFR_5 = new Color(128, 40, 128);
        public static Color TEAM_SFR_5_CHAT_MESSAGE = TEAM_SFR_5 * 1.45f;
        public static Color TEAM_SFR_5_CHAT_NAME = new Color(255, 90, 255);
        public static Color TEAM_SFR_5_CHAT_TAG = new Color(255, 125, 255);

        public static Color TEAM_SFR_6 = new Color(0, 112, 112);
        public static Color TEAM_SFR_6_CHAT_MESSAGE = TEAM_SFR_6 * 1.4f;
        public static Color TEAM_SFR_6_CHAT_NAME = new Color(10, 230, 230);
        public static Color TEAM_SFR_6_CHAT_TAG = new Color(125, 255, 255);

        public static Color TEAM_6 = new Color(170, 170, 170);
        public static Color TEAM_6_CHAT_MESSAGE = TEAM_6 * 1.4f;
        public static Color TEAM_6_CHAT_NAME = new Color(240, 240, 240);
        public static Color TEAM_6_CHAT_TAG = new Color(210, 210, 210);

        public static Color TEAM_SPECTATOR = new Color(85, 85, 85);
        public static Color TEAM_SPECTATOR_CHAT_MESSAGE = new Color(120, 120, 120);
        public static Color TEAM_SPECTATOR_CHAT_NAME = new Color(150, 150, 150);
        public static Color TEAM_SPECTATOR_CHAT_TAG = new Color(124, 124, 124);
    }

    public const int TEAM_SPECTATOR_INDEX = 128;
    public const int GAMESLOT_SPECTATOR_INDEX = 128;
    public static Texture2D TEAM_5_ICON = null;
    public static Texture2D TEAM_6_ICON = null;
    public static Texture2D TEAM_SPECTATOR_ICON = null;

    public static GameSlot SPECTATOR_GAMESLOT = new GameSlot(GAMESLOT_SPECTATOR_INDEX)
    {
        GameSlotIndex = GAMESLOT_SPECTATOR_INDEX,
        FillWithBotAI = SFDGameScriptInterface.PredefinedAIType.None,
        GameUser = null,
        CurrentTeam = (Team)TEAM_SPECTATOR_INDEX,
    };

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
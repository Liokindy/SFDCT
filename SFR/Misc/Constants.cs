using HarmonyLib;
using SFD;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using CSettings= SFDCT.Settings.Values;

namespace SFDCT.Misc;

public static class Constants
{
    public readonly struct Paths
    {
        public static string SFDCT = Path.Combine(Program.GameDirectory, "SFDCT");
        public static string Content = Path.Combine(Paths.SFDCT, "Content");
        public static string ConfigurationIni = Path.Combine(Paths.SFDCT, "config.ini");
        public static string Profiles = Path.Combine(Content, "Profile");
    }
    public readonly struct Version
    {
        public static string SFD = "v.1.3.7d";
        public static string SFDCT = "v.1.0.4";
        public static bool InDev = true;
        public static string Label
        {
            get
            {
                return SFDCT + (InDev ? " (Dev)" : "");
            }
        }
    }
    public struct Colors
    {
        public static Color OmenFlash = Color.White;
        public static Color OmenBar = new(120, 152, 255);
    }

    public static int SlotCount
    {
        get
        {
            if (HOST_GAME_EXTENDED_SLOTS && GameSFD.Handle.ImHosting)
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
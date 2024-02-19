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
        public static string SFDCT = "v.1.0.2";
    }
    public struct Colors
    {
        public static Color OmenFlash = Color.White;
        public static Color OmenBar = new(120, 152, 255);
    }
    public static bool SetSlots(int num)
    {
        if (num < 8 || num > 32)
        {
            return false;
        }

        HOST_GAME_SLOT_COUNT = num;
        HOST_GAME_SLOT_STATES = new byte[num];
        HOST_GAME_SLOT_TEAMS = new Team[num];
        return true;
    }

    public static int SlotCount
    {
        get
        {
            if (GameSFD.Handle.ImHosting && HOST_GAME_SLOT_COUNT > 8)
            {
                return HOST_GAME_SLOT_COUNT;
            }
            return HOST_GAME_SLOT_COUNT;
        }
    }

    public static int HOST_GAME_SLOT_COUNT = 8;
    public static byte[] HOST_GAME_SLOT_STATES = new byte[HOST_GAME_SLOT_COUNT];
    public static Team[] HOST_GAME_SLOT_TEAMS = new Team[HOST_GAME_SLOT_COUNT];
}
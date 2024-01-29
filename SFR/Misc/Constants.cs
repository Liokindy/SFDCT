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
        public static string SFDCT = "v.1.0.1";
    }
    public struct Security
    {
        public static bool CanUseObfuscatedNames
        {
            get
            {
                if (!CSettings.GetBool("USE_OBFUSCATED_HOST_ACCOUNT_NAME"))
                {
                    return false;
                }
                if (RealPersonaName.Length > 32 || GameSFD.Handle.cracky || GameSFD.Handle.m_cracky_initialized || GameSFD.Handle.m_cracky_kicky)
                {
                    return false;
                }
                if (GameSFD.Handle.Client == null)
                {
                    return false;
                }
                if (GameSFD.Handle.Client.CurrentIPEndpoint != null)
                {
                    if (GameSFD.Handle.ImHosting || GameSFD.Handle.Client.CurrentIPEndpoint.ToString().Contains("127.0.0.1"))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
        public static string RealPersonaName = "";
        public static bool ValidateObfuscatedName(string name, out string errorMessage)
        {
            if (string.IsNullOrEmpty(name))
            {
                errorMessage = "Empty or null";
                return true;
            }
            if (name.Length > 32)
            {
                errorMessage = "Length is over Steam's max account name length (32 characters)";
                return false;
            }
            if (name.ToUpper() == "GURT" || name.ToUpper() == "COM" || name.ToUpper() == "HJARPE")
            {
                errorMessage = "Name is reserved";
                return false;
            }
            errorMessage = "";
            return true;
        }
    }

    public static bool SetSlots(int num)
    {
        if (num < 8 || num > 32)
        {
            // Slowdown cowboy!
            return false;
        }

        HOST_GAME_SLOT_COUNT = num;
        HOST_GAME_SLOT_STATES = new byte[num];
        HOST_GAME_SLOT_TEAMS = new Team[num];
        return true;
    }

    public static int HOST_GAME_SLOT_COUNT = 8;
    public static byte[] HOST_GAME_SLOT_STATES = new byte[HOST_GAME_SLOT_COUNT];
    public static Team[] HOST_GAME_SLOT_TEAMS = new Team[HOST_GAME_SLOT_COUNT];
}
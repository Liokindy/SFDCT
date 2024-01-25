using HarmonyLib;
using SFD;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using CSettings= SFDCT.Settings.Values;

namespace SFDCT.Misc;

public static class Constants
{
    public struct Paths
    {
        public static string Custom = Path.Combine(Program.GameDirectory, @"SFDCT");
        public static string ConfigurationIni = Path.Combine(Paths.Custom, @"config.ini");
    }
    public struct Version
    {
        public const string SFD = "v.1.3.7d";
        public const string SFDCT = "v.1.0.0a";
    }
    public struct Security
    {
        public static bool CanUseObfuscatedNames
        {
            get
            {
                if (RealPersonaName.Length > 32 || GameSFD.Handle.cracky || GameSFD.Handle.m_cracky_initialized || GameSFD.Handle.m_cracky_kicky)
                {
                    return false;
                }
                if (!CSettings.GetBool("USE_OBFUSCATED_HOST_ACCOUNT_NAME"))
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
        public static bool ValidateObfuscatedName(string name, out string resultName, out string errorMessage)
        {
            if (string.IsNullOrEmpty(name))
            {
                resultName = "Unnamed";
                errorMessage = "Empty or null";
                return true;
            }
            if (name.Length > 32)
            {
                resultName = name.Substring(0, 32);
                errorMessage = "Length is over Steam's max account name length (32 characters)";
                return false;
            }
            if (name.ToUpper() == "GURT" || name.ToUpper() == "COM" || name.ToUpper() == "HJARPE")
            {
                resultName = "Unnamed";
                errorMessage = "Name is reserved";
                return false;
            }
            resultName = name;
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
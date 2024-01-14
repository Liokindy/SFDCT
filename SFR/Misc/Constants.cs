using HarmonyLib;
using SFD;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using SFRSettings = SFDCT.Settings.Values;

namespace SFDCT.Misc;

public static class Constants
{
    // public const string SFRVersion = "v.1.3.7d"; // "v.1.0.3b_dev";
    // internal static readonly string ServerVersion = "v.1.3.7d";// SFRVersion.Replace("v.1", "v.2");
    // internal static readonly Random Random = new();
    public struct Paths
    {
        public static string Custom = Path.Combine(Program.GameDirectory, @"SFDCT");
        public static string ConfigurationIni = Path.Combine(Paths.Custom, @"config.ini");
    }
    public struct Version
    {
        public const string SFD = "v.1.3.7d";
        public const string SFDCT = "v.1.0.0";
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
                if (!SFRSettings.GetBool("USE_OBFUSCATED_HOST_ACCOUNT_NAME"))
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
    
    public struct Colors
    {
        public static Color Outline_Team_Independent = new Color(192, 192, 192);
        public static Color Outline_Team_1 = new Color(192, 192, 255);
        public static Color Outline_Team_2 = new Color(255, 168, 168);
        public static Color Outline_Team_3 = new Color(144, 255, 144);
        public static Color Outline_Team_4 = new Color(240, 240, 128);
        // public static Color Outline_Team_5 = new Color(240, 187, 255);
        // public static Color Outline_Team_6 = new Color(128, 239, 255);
        public static Color Outline_Team_AllyBlue = Outline_Team_1;
        public static Color Outline_Team_AllyGreen = Outline_Team_3;
        public static Color Outline_Team_EnemyRed = Outline_Team_2;
    }
}
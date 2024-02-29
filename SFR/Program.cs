using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
using SFDCT.Helper;
using SFDCT.Misc;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT. This class will decide what game to start, SFDCT, vanilla-SFD, or SFR (if available).
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    internal static readonly string GitHubRepositoryURL = "https://github.com/Liokindy/SFDCT/";
    internal static readonly string GitHubVersionFileURL = "https://raw.githubusercontent.com/Liokindy/SFDCT/master/ModVersion.txt";
    private static readonly Harmony Harmony = new("SuperfightersDeluxe_Custom");

    private static int Main(string[] args)
    {
        string SFRDirectory = Path.Combine(GameDirectory, "SFR");
        string SFRExecutable = Path.Combine(GameDirectory, "SFR.exe");
        bool hasSFR = Directory.Exists(SFRDirectory) && File.Exists(SFRExecutable);

        // Help stuff for normies
        if (args.Contains("-help", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("ALL LAUNCH ARGUMENTS:");
            Logger.LogWarn("-HELP           Show this message.");
            Logger.LogWarn("-SFD            Start Superfighters Deluxe.");

            if (hasSFR)
            {
            Logger.LogWarn("-SFR            Start Superfighters Redux.");
            }

            Logger.LogWarn("-SLOTS [8-32]   Use extended-slots.");
            return 0;
        }

        // Launch SFR, if found
        if (args.Contains("-SFR", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("Starting Superfighters Redux...");

            if (hasSFR)
            {
                Process.Start(SFRExecutable, string.Join(" ", args));
            }
            else
            {
                Logger.LogError("SFR is not installed or could not be found");
            }
            return 0;
        }

        // Launch SFD
        if (args.Contains("-SFD", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("Starting Superfighters Deluxe...");
           
            string SFDExectuable = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
            if (File.Exists(SFDExectuable))
            {
                Process.Start(SFDExectuable, string.Join(" ", args));
            }
            else
            {
                Logger.LogError("Could not find Superfighters Deluxe's executable");
            }
            return 0;
        }

        // Check extended-slots
        if (args.Contains("-slots", StringComparer.OrdinalIgnoreCase))
        {
            int slotArgCount = 8;
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-slots", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > i + 1)
                    {
                        if (!int.TryParse(args[i + 1], out slotArgCount))
                        {
                            slotArgCount = 8;
                        }
                    }
                    break;
                }
            }

            if (slotArgCount == 8)
            {
                Logger.LogWarn("Select slot count (8-32): ", false);
                string line = Console.ReadLine();
                if (!int.TryParse(line, out slotArgCount))
                {
                    slotArgCount = 8;
                }
            }

            slotArgCount = Math.Max(Math.Min(slotArgCount, 32), 8);
            if (slotArgCount != 8)
            {
                Logger.LogWarn($"Setting max-slots to {slotArgCount}");
                Constants.HOST_GAME_EXTENDED_SLOTS = true;
                Constants.HOST_GAME_SLOT_COUNT = slotArgCount;
                Constants.HOST_GAME_SLOT_STATES = new byte[Constants.HOST_GAME_SLOT_COUNT];
                Constants.HOST_GAME_SLOT_TEAMS = new SFD.Team[Constants.HOST_GAME_SLOT_COUNT];
            }
        }


        CheckRepositoryVersion();
       
        // Patch SFD and start SFDCT
        Logger.LogInfo("Starting SFDCT...");
        Harmony.PatchAll();
        Logger.LogInfo("Patching completed...");
        SFD.Program.Main(args);
        
        return 0;
    }

    /// <summary>
    ///     Fetch "ModVersion.txt" from the repository and compare versions against ours.
    /// </summary>
    public static void CheckRepositoryVersion()
    {
        Logger.LogWarn("CHECKING VERSIONS FROM GITHUB REPOSITORY...");

        WebClient wc = null;
        try
        {
            wc = new();
            string versionFile = wc.DownloadString(GitHubVersionFileURL);

            string[] foundVersions = versionFile.Split('|');
            string latestVersion = foundVersions[0];
            string previewVersion = foundVersions[1];

            string targetVersion = Constants.Version.InDev ? previewVersion : latestVersion;

            if (Constants.Version.SFDCT != targetVersion)
            {
                Logger.LogError($"Current version ({Constants.Version.SFDCT}) differs from repository ({targetVersion})");
                Logger.LogError($"Get the latest release at \"{GitHubRepositoryURL}\"");
            }
            else
            {
                Logger.LogWarn($"Current version ({Constants.Version.SFDCT}) matches repository ({targetVersion})");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error while getting repository's version - {ex.Message}");
        }
        wc?.Dispose();
    }

    /// <summary>
    ///     Some values set in the config.ini might crash the game when it boots without any patching. This
    ///     class will check for those values and revert them to a normal vanilla-range, however, this
    ///     does requires the user to boot the game atleast once with SFDCT. 
    /// </summary>
    public static void RevertVanillaSFDConfig()
    {
        Logger.LogDebug("Checking to revert values in SFD's config.ini...");
        string documentsFolder = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Superfighters Deluxe"));
        string pathToSFDConfig = Path.Combine(documentsFolder, "config.ini");
        bool needsToSave = false;

        if (File.Exists(pathToSFDConfig))
        {
            Logger.LogDebug("Found Superfighters Deluxe/config.ini");
            string[] iniLines = File.ReadAllLines(pathToSFDConfig, Encoding.Default);
            for (int index = 0; index < iniLines.Length; index++)
            {
                string line = iniLines[index];

                // PLAYER_[1-8]_PROFILE will crash the game if set to above 8
                if (line.StartsWith("PLAYER_"))
                {
                    int.TryParse(line.Substring(7, 1), out int playerSlot);
                    int.TryParse(line.Substring(17), out int profileSlot);

                    if (profileSlot >= 9)
                    {
                        Logger.LogDebug($"Reverting 'PLAYER_{playerSlot}_PROFILE={profileSlot}'...");
                        iniLines[index] = line.Substring(0, 17) + "0";
                        needsToSave = true;
                    }
                }
            }

            if (needsToSave)
            {
                Logger.LogDebug("Saving...");
                using StreamWriter sw = new(pathToSFDConfig, false, Encoding.Default);
                foreach (string line in iniLines)
                {
                    sw.WriteLine(line);
                }
                sw.Close();
            }
        }

    }
}
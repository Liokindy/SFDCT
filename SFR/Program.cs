using System;
using System.Drawing;
using Process = System.Diagnostics.Process;
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

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT. This class will decide what game to start, SFDCT, vanilla-SFD, or SFR (if available).
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    internal static Icon GameIcon;
    private static readonly Harmony Harmony = new("SuperfightersDeluxe_Custom");
    private static int Main(string[] args)
    {
        int gameSelection = 0;
        bool skipSelection = args.Contains("-SKIP") || args.Contains("-SFDCT") || args.Contains("-SFD") || args.Contains("-SFR");
        bool hasSFR = File.Exists(Path.Combine(GameDirectory, "SFR.exe")) && Directory.Exists(Path.Combine(GameDirectory, "SFR"));

        if (!skipSelection)
        {
            Logger.LogDebug("Choose game.");
            Logger.LogWarn("0. SFDCT; 1. SFD" + (hasSFR ? "; 2. SFR" : ""));
            Logger.LogWarn("Start option: ", false);
            DateTime selectionStart = DateTime.Now;
            ConsoleKey selection = Console.ReadKey().Key;
            Console.SetCursorPosition(0, Console.CursorTop + 1);

            if (selection == ConsoleKey.D1 || selection == ConsoleKey.NumPad1)
            {
                gameSelection = 1;
            }
            else if (hasSFR && selection == ConsoleKey.D2 || selection == ConsoleKey.NumPad2)
            {
                gameSelection = 2;
            }
        }
        else
        {
            // In order
            if (args.Contains("-SFD"))
            {
                gameSelection = 1;
            }
            else if (args.Contains("-SFR") && hasSFR)
            {
                gameSelection = 2;
            }
        }


        // Check what game to boot
        try
        {
            switch(gameSelection)
            {
                case 1:
                    Logger.LogInfo("Starting Superfighters Deluxe");
                    RevertVanillaSFDConfig();
                    string SFD_exe = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
                    Process.Start(SFD_exe, string.Join(" ", args));
                    break;
                case 2:
                    Logger.LogInfo("Starting Superfighters Redux");
                    RevertVanillaSFDConfig();
                    string SFR_exe = Path.Combine(GameDirectory, "SFR.exe");
                    Process.Start(SFR_exe, "-SFR " + string.Join(" ", args));
                    break;
                default:
                    // Set the cmd title
                    Console.Title = "Superfighters Deluxe Custom Console";

                    // Get SFDCT icon
                    try
                    {
                        GameIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
                    }
                    catch
                    {
                        GameIcon = null;
                    }

                    // Max-slots
                    if (args.Contains("-SLOTS"))
                    {
                        int slotArgIndex = args.ToList().IndexOf("-SLOTS");
                        if (args.Length > slotArgIndex)
                        {
                            if (int.TryParse(args[slotArgIndex + 1], out int slotArgValue))
                            {
                                if (Constants.SetSlots(slotArgValue))
                                {
                                    Logger.LogWarn($"SETTING MAX-SLOTS TO {slotArgValue}");
                                    Logger.LogInfo("Maps and scripts may break while having more than 8 slots.");
                                    Logger.LogInfo("You can use the \"/sslot\" command in chat to set a slot state in-game.");
                                    Logger.LogInfo("You can also use the \"/slots\" command to list the states of all slots.");
                                    Logger.LogDebug("\"/sslot 1 EXPERT TEAM2\", set the 2nd slot to an expert bot in team 2.");
                                    Logger.LogDebug("\"/sslot 8 EASY TEAM4\", set the 9th slot to an easy bot in team independent.");
                                    Logger.LogDebug("\"/sslot 6 EXPERT\", set the 7th slot to an expert bot without changing team.");
                                    Logger.LogDebug("\"/sslot 15 CLOSED\", set the 16th slot as closed.");
                                }
                                else
                                {
                                    Logger.LogError($"Max-slots parameter can only be between 8 and 32.");
                                }
                            }
                        }
                    }

                    // Patch and start SFD
                    Logger.LogInfo("Starting Superfighters Deluxe Custom");
                    Harmony.PatchAll();
                    Logger.LogInfo("Patching completed, starting...");
                    SFD.Program.Main(args);
                    break;
            }
        }
        catch(Exception ex)
        {
            Logger.LogError("Exception trying to start game - " + ex.Message);
            return -1;
        }

        return 0;
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
    /*
    private static bool CheckUpdate()
    {
        string remoteVersion;
        try
        {
            _webClient = new WebClient();
            remoteVersion = _webClient.DownloadString(VersionURI);
        }
        catch (WebException)
        {
            Logger.LogError("Couldn't fetch updates - Starting the game without updating!");
            _webClient.Dispose();
            return false;
        }

        if (remoteVersion != Constants.SFRVersion)
        {
            _gameURI = _gameURI.Replace("GAMEVERSION", remoteVersion);
            return Update();
        }

        _webClient.Dispose();

        Logger.LogInfo("No updates found. Starting");
        return false;
    }
    */
    /*
    private static void ReplaceOldFile(string file)
    {
        string newExtension = Path.ChangeExtension(file, "old");
        if (File.Exists(newExtension))
        {
            File.Delete(newExtension);
        }

        File.Move(file, newExtension);
    }

    private static bool Update()
    {
        string contentDirectory = Path.Combine(GameDirectory, @"SFR");
        if (Choice($"All files in {contentDirectory} will be erased. Proceed? (Y/n):"))
        {
            Logger.LogInfo("Downloading files...");
            string archivePath = Path.Combine(GameDirectory, "SFDCT.zip");

            try
            {
                _webClient.DownloadFile(_gameURI, archivePath);
            }
            catch (WebException)
            {
                Logger.LogError("Couldn't fetch updates - Starting the game without updating!");
                return false;
            }
            finally
            {
                _webClient.Dispose();
            }

            ReplaceOldFile(Assembly.GetExecutingAssembly().Location);
            ReplaceOldFile(Path.Combine(GameDirectory, "SFDCT.exe.config"));

            foreach (string file in Directory.GetFiles(contentDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                ReplaceOldFile(file);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(contentDirectory, "Content"), "*.*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                archive.ExtractToDirectory(GameDirectory);
                archive.Dispose();
            }

            File.Delete(archivePath);

            Logger.LogInfo("SFR has been updated to the latest version.");
            return true;
        }

        _webClient.Dispose();

        Logger.LogWarn("Ignoring update.");
        return false;
    }
    */
}
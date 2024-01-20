using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SFDCT.Helper;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT. This class will simply check for available updates, patch SFD and start the game.
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    internal static Icon GameIcon;
    private static readonly Harmony Harmony = new("SuperfightersDeluxeCustom");
    private static int Main(string[] args)
    {
        try
        {
            GameIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }
        catch(Exception ex)
        {
            Logger.LogError("Exception trying to get app icon - " + ex.Message);
        }

        Logger.LogInfo("Patching...");
        Harmony.PatchAll();
        Logger.LogInfo("Starting...");
        SFD.Program.Main(args);

        return 0;
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
using HarmonyLib;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT.
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    internal static readonly string GitHubRepositoryURL = "https://github.com/Liokindy/SFDCT";
    internal static readonly string GitHubRepositoryVersionFileURL = "https://raw.githubusercontent.com/Liokindy/SFDCT/master/version";

    internal static readonly Harmony Harmony = new("github.com/Liokindy/SFDCT");
    internal static bool DebugMode = false;

    private static int Main(string[] args)
    {
        if (args.Contains("-HELP", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("Launch parameters");
            Logger.LogWarn("- HELP            Show this help message");
            Logger.LogWarn("- SKIP            Skip version fetching");
            Logger.LogWarn("- SFD             Start SFD");
            Logger.LogWarn("- DEBUG           Run in debug mode, shows debug messages");
            return 0;
        }

        if (args.Contains("-SFD", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("Starting SFD...");

            string sfdFile = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
            if (File.Exists(sfdFile))
            {
                Process.Start(sfdFile, string.Join(" ", args));
                return 0;
            }

            Logger.LogError("SFD file not found");
            return -1;
        }

        if (args.Contains("-DEBUG", StringComparer.OrdinalIgnoreCase))
        {
            DebugMode = true;
        }

#if DEBUG
        DebugMode = true;
#endif

        if (DebugMode)
        {
            Logger.LogWarn("--------------------------------");
            Logger.LogWarn("RUNNING IN DEBUG MODE");
            Logger.LogWarn("DEBUG MESSAGES WILL APPEAR");
            Logger.LogWarn("--------------------------------");
        }

        Stopwatch sw = new();
        sw.Start();

        if (!args.Contains("SKIP", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("--------------------------------");
            Logger.LogWarn("Fetching repository version...");

            WebClient webClient;
            try
            {
                webClient = new();

                string repositoryVersion = webClient.DownloadString(GitHubRepositoryVersionFileURL).Trim();
                if (!Globals.Version.SFDCT.Equals(repositoryVersion, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarn("Current version differs from repository!");
                    Logger.LogWarn($"Current {Globals.Version.SFDCT} - Repository {repositoryVersion}");
                    Logger.LogWarn("Get a new release at:");
                    Logger.LogWarn($"- {GitHubRepositoryURL}");
                }
            }
            catch (Exception)
            {
                Logger.LogError("Failed to fetch repository version");
                Logger.LogError("To skip version fetching, use -skip");
                webClient = null;
            }
            Logger.LogWarn("--------------------------------");
        }

        Logger.LogInfo("Current SFDCT version is: " + Globals.Version.SFDCT);
        Logger.LogInfo("Target SFD version is: " + Globals.Version.SFD);

        Logger.LogInfo("Initializing SFDCT...");
        IniFile.Initialize();

        Logger.LogInfo("Patching SFD...");
        Harmony.PatchAll();

        sw.Stop();
        Logger.LogInfo($"Initialized and Patched in {sw.ElapsedMilliseconds}ms");
        sw = null;

        Logger.LogInfo("Starting SFD...");
        SFD.Program.Main(args);

        return 0;
    }
}